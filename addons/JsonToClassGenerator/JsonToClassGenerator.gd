@tool
extends EditorPlugin

var panel_container: Control
var path_line_edit: LineEdit
var status_label: Label

const DEFAULT_OUTPUT_DIR = "Scripts/GeneratedClasses"
const CONFIG_MANAGER_DIR = "Scripts/Tools"
const NAMESPACE = "MyProject"

func _enter_tree() -> void:
	# 创建主面板容器
	panel_container = VBoxContainer.new()
	panel_container.name = "JsonToClassGeneratorPanel"

	# 标题
	var title_label := Label.new()
	title_label.text = "JSON → C# 类生成器"
	title_label.add_theme_font_size_override("font_size", 16)
	panel_container.add_child(title_label)

	# 分隔线
	panel_container.add_child(HSeparator.new())

	# 路径输入栏区域
	var path_hbox := HBoxContainer.new()
	var path_label := Label.new()
	path_label.text = "文件夹路径:"
	path_label.custom_minimum_size.x = 80
	path_label.vertical_alignment = 1
	path_hbox.add_child(path_label)

	path_line_edit = LineEdit.new()
	path_line_edit.placeholder_text = "例如: Config"
	path_line_edit.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	path_line_edit.text = "Config"
	path_hbox.add_child(path_line_edit)

	# 浏览按钮
	var browse_button := Button.new()
	browse_button.text = "浏览..."
	browse_button.pressed.connect(_on_browse_pressed)
	path_hbox.add_child(browse_button)

	panel_container.add_child(path_hbox)

	# 生成按钮
	var generate_button := Button.new()
	generate_button.text = "生成"
	generate_button.custom_minimum_size.y = 36
	generate_button.pressed.connect(_on_generate_pressed)
	panel_container.add_child(generate_button)

	# 状态标签
	status_label = Label.new()
	status_label.text = "就绪。点击「生成」开始。"
	status_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	status_label.custom_minimum_size.y = 60
	panel_container.add_child(status_label)

	# 将面板添加到编辑器的底部面板
	add_control_to_bottom_panel(panel_container, "JSON→C#")


func _exit_tree() -> void:
	if panel_container:
		remove_control_from_bottom_panel(panel_container)
		panel_container.queue_free()


func _on_browse_pressed() -> void:
	# 使用 Godot 内置的文件对话框选择文件夹
	var dialog := FileDialog.new()
	dialog.file_mode = FileDialog.FILE_MODE_OPEN_DIR
	dialog.access = FileDialog.ACCESS_RESOURCES
	dialog.title = "选择包含 JSON 文件的文件夹"
	dialog.current_dir = path_line_edit.text if path_line_edit.text.strip_edges() != "" else "res://"
	dialog.dir_selected.connect(func(dir: String) -> void:
		# 将绝对 res:// 路径转为相对路径显示
		path_line_edit.text = dir.replace("res://", "")
	)
	dialog.canceled.connect(func() -> void: dialog.queue_free())
	dialog.dir_selected.connect(func(_d: String) -> void: dialog.queue_free())
	# 需要将 dialog 添加到场景树才能显示
	get_editor_interface().get_base_control().add_child(dialog)
	dialog.popup_centered_ratio(0.5)


func _on_generate_pressed() -> void:
	var folder_path := path_line_edit.text.strip_edges()
	if folder_path == "":
		_set_status("❌ 请输入文件夹路径", true)
		return

	# 标准化为 res:// 路径
	var res_path := _to_res_path(folder_path)
	var dir := DirAccess.open(res_path)
	if dir == null:
		_set_status("❌ 无法打开文件夹: %s\n请确认路径正确（相对于项目根目录）" % folder_path, true)
		return

	# 收集所有 JSON 文件
	var json_files: Array[String] = []
	dir.list_dir_begin()
	var file_name := dir.get_next()
	while file_name != "":
		if not dir.current_is_dir() and file_name.get_extension().to_lower() == "json":
			json_files.append(file_name)
		file_name = dir.get_next()
	dir.list_dir_end()

	if json_files.is_empty():
		_set_status("⚠️ 文件夹 %s 中没有找到 JSON 文件" % folder_path, true)
		return

	# 确保输出目录存在
	var output_dir := "res://%s" % DEFAULT_OUTPUT_DIR
	DirAccess.make_dir_recursive_absolute(output_dir)

	var generated_count := 0
	var error_messages: Array[String] = []
	var generated_class_names: Array[String] = []

	for json_file in json_files:
		var full_path := "%s/%s" % [res_path, json_file]
		var cs_class_name := _to_pascal_case(json_file.get_basename())
		var err := _generate_class_from_json(full_path, json_file.get_basename())
		if err == "":
			generated_count += 1
			generated_class_names.append(cs_class_name)
		else:
			error_messages.append("%s: %s" % [json_file, err])

	# 生成 ConfigManager 单例
	var cm_err := _generate_config_manager(generated_class_names)

	var msg := "✅ 成功生成 %d 个 C# 类文件 → %s/" % [generated_count, DEFAULT_OUTPUT_DIR]
	if cm_err == "":
		msg += "\n✅ ConfigManager 单例已更新 → %s/ConfigManager.cs" % CONFIG_MANAGER_DIR
	else:
		msg += "\n❌ ConfigManager 生成失败: %s" % cm_err
	if not error_messages.is_empty():
		msg += "\n❌ 以下文件出错:\n" + "\n".join(error_messages)

	_set_status(msg, not error_messages.is_empty() or cm_err != "")

	# 刷新文件系统以便在编辑器中看到新文件
	get_editor_interface().get_resource_filesystem().scan()


## 将用户输入的路径转换为 res:// 格式
func _to_res_path(path: String) -> String:
	path = path.strip_edges()
	if path.is_empty():
		return "res://"
	if path.begins_with("res://"):
		return path
	# 去掉开头的斜杠
	while path.begins_with("/"):
		path = path.substr(1)
	return "res://%s" % path


func _set_status(msg: String, is_error: bool) -> void:
	status_label.text = msg
	if is_error:
		status_label.add_theme_color_override("font_color", Color(1, 0.3, 0.3))
	else:
		status_label.add_theme_color_override("font_color", Color(0.3, 0.8, 0.3))


## 从单个 JSON 文件生成 C# 类
## 返回空字符串表示成功，否则返回错误信息
func _generate_class_from_json(json_path: String, file_base_name: String) -> String:
	# 读取文件
	var file_access := FileAccess.open(json_path, FileAccess.READ)
	if file_access == null:
		return "无法打开文件 (error: %d)" % FileAccess.get_open_error()

	var json_text := file_access.get_as_text()
	file_access.close()

	# 解析 JSON
	var json := JSON.new()
	var parse_err := json.parse(json_text)
	if parse_err != OK:
		return "JSON 解析失败 (行 %d): %s" % [json.get_error_line(), json.get_error_message()]

	var data: Variant = json.data

	# 判断 JSON 根类型：
	# - 数组：取第一个元素的字段作为类的字段定义
	# - 对象：直接使用该对象的字段
	var sample: Dictionary
	if data is Array:
		if data.is_empty():
			return "JSON 数组为空，无法推断字段"
		var first = data[0]
		if first is Dictionary:
			sample = first
		else:
			return "JSON 数组元素不是对象"
	elif data is Dictionary:
		sample = data
	else:
		return "JSON 根节点既不是数组也不是对象"

	# 生成类名（PascalCase）
	var cs_class_name := _to_pascal_case(file_base_name)

	# 收集所有字段，合并多个对象中的字段（如果是数组）
	# 同时收集每个字段在所有数据项中的全部值，用于精确判断数组元素类型
	var field_values: Dictionary = {}  # key: 字段名, value: Array[Variant]
	if data is Array:
		for item in data:
			if item is Dictionary:
				for key in item:
					if not sample.has(key):
						sample[key] = item[key]
					# 收集该字段的所有值
					if not field_values.has(key):
						field_values[key] = []
					field_values[key].append(item[key])

	# 生成 C# 类代码（传入 json_text 和 field_values 用于精确判断类型）
	var cs_code := _build_csharp_class(cs_class_name, sample, json_text, field_values)

	# 写入文件
	var output_file := "res://%s/%s.cs" % [DEFAULT_OUTPUT_DIR, cs_class_name]
	var fa := FileAccess.open(output_file, FileAccess.WRITE)
	if fa == null:
		return "无法写入输出文件: %s" % output_file
	fa.store_string(cs_code)
	fa.close()

	return ""


## 将文件名 / 字段名转换为 PascalCase
## 例如: place_info → PlaceInfo, item_name → ItemName
func _to_pascal_case(text: String) -> String:
	if text == "":
		return "GeneratedClass"

	# 按下划线和空格分割
	var parts := text.split("_")
	var parts2 := []
	for p in parts:
		for sub in p.split(" "):
			if sub != "":
				parts2.append(sub)

	var result := ""
	for part in parts2:
		if part.length() > 0:
			# 首字母大写，其余保持原样（处理 camelCase 输入）
			result += part[0].to_upper() + part.substr(1)

	# 确保类名以字母开头（C# 要求）
	if result.length() > 0 and not result[0].is_valid_identifier():
		result = "C" + result

	return result if result != "" else "GeneratedClass"


## 将 JSON 字段名转换为 PascalCase 属性名
func _to_property_name(field_name: String) -> String:
	return _to_pascal_case(field_name)


## 根据 JSON 值推断 C# 类型
## json_text: 原始 JSON 文本，用于精确判断数字是 int 还是 double
## value: Godot JSON 解析后的值（仅用于类型大类判断）
## field_name: 字段名
func _infer_csharp_type(value: Variant, json_text: String, field_name: String) -> String:
	match typeof(value):
		TYPE_BOOL:
			return "bool"
		TYPE_INT:
			return "int"
		TYPE_FLOAT:
			# Godot JSON 解析器将所有数字都转成 float
			# 需要检查原始 JSON 文本中该字段是否为整数格式（不含小数点）
			if _json_field_is_int(json_text, field_name):
				return "int"
			return "double"
		TYPE_STRING:
			return "string"
		TYPE_ARRAY:
			var arr := value as Array
			if arr.is_empty():
				return "List<object>"
			# 遍历数组中所有元素，按优先级判断：
			# 有任何字符串 → string
			# 全是数字，有任何小数 → double
			# 全为整数 → int
			var has_string := false
			var has_double := false
			var has_int := false
			for elem in arr:
				match typeof(elem):
					TYPE_STRING:
						has_string = true
					TYPE_FLOAT:
						# Godot 把所有数字当 float，用原始 JSON 判断
						if _array_elem_has_decimal(json_text, field_name):
							has_double = true
						else:
							has_int = true
					TYPE_INT:
						has_int = true
					_:
						pass  # bool, dict 等不影响判断
			var elem_type := "object"
			if has_string:
				elem_type = "string"
			elif has_double:
				elem_type = "double"
			elif has_int:
				elem_type = "int"
			return "List<%s>" % elem_type
		TYPE_DICTIONARY:
			return "Dictionary<string, object>"
		TYPE_NIL:
			return "object"
		_:
			return "object"


## 检查 JSON 文本中指定字段的值是否为纯整数格式（不含小数点）
## 通过正则匹配 "field_name": 数字 来判断
func _json_field_is_int(json_text: String, field_name: String) -> bool:
	var regex := RegEx.new()
	# 匹配 "fieldName": 数字 的模式，捕获数字部分
	# 支持带引号的字段名，数字可能是正负整数或小数
	regex.compile("\"" + field_name + "\"" + "\\s*:\\s*(-?\\d+)(?:\\.\\d+)?")
	var result := regex.search(json_text)
	if result == null:
		return false
	# 如果匹配到的数字捕获组存在，说明是纯整数
	# 注意：正则会捕获整数部分，如果后面跟着 .数字 则整个匹配会包含小数部分
	# 这里更稳健的做法是检查完整匹配是否包含小数点
	var full_match := result.get_string()
	return not ("." in full_match.substr(full_match.find(":") + 1))


## 判断 JSON 文本中数组字段是否包含小数点数字
## 搜索 "field_name": [ 后直到 ] 之间的所有数字，检测是否有小数点
func _array_elem_has_decimal(json_text: String, field_name: String) -> bool:
	var regex := RegEx.new()
	# 匹配 "field_name": [...] 整个数组内容
	regex.compile("\"" + field_name + "\"" + "\\s*:\\s*\\[([^\\]]*)\\]")
	var result := regex.search(json_text)
	if result == null:
		return false
	var array_content := result.get_string(1)
	if array_content == "":
		return false
	# 检查数组内容中是否有小数点数字
	var num_regex := RegEx.new()
	num_regex.compile("-?\\d+\\.\\d+")
	return num_regex.search(array_content) != null


## 构建 C# 类代码字符串
func _build_csharp_class(cs_class_name: String, sample: Dictionary, json_text: String, field_values: Dictionary) -> String:
	var lines: Array[String] = []

	lines.append("using System;")
	lines.append("using System.Collections.Generic;")
	lines.append("")
	lines.append("namespace %s" % NAMESPACE)
	lines.append("{")
	lines.append("    /// <summary>")
	lines.append("    /// 自动生成的数据类，对应 %s JSON 配置" % cs_class_name)
	lines.append("    /// 由 JsonToClassGenerator 插件生成")
	lines.append("    /// </summary>")
	lines.append("    public class %s" % cs_class_name)
	lines.append("    {")

	var keys := sample.keys()
	for i in range(keys.size()):
		var key: String = keys[i]
		var value: Variant = sample[key]

		var cs_type := _infer_csharp_type(value, json_text, key)
		var prop_name := _to_property_name(key)

		# 添加注释标注原始 JSON 字段名
		lines.append("        /// <summary>原始字段: %s</summary>" % key)
		lines.append("        public %s %s { get; set; }" % [cs_type, prop_name])

		# 非最后一个字段时添加空行
		if i < keys.size() - 1:
			lines.append("")

	lines.append("    }")
	lines.append("}")
	lines.append("")

	return "\n".join(lines)


## 生成 ConfigManager 单例脚本
## 返回空字符串表示成功，否则返回错误信息
func _generate_config_manager(class_names: Array[String]) -> String:
	if class_names.is_empty():
		return "没有可用的类名"

	# 确保输出目录存在
	var output_dir := "res://%s" % CONFIG_MANAGER_DIR
	DirAccess.make_dir_recursive_absolute(output_dir)

	var lines: Array[String] = []
	lines.append("using System;")
	lines.append("using System.Collections.Generic;")
	lines.append("using %s.Tools;" % NAMESPACE)
	lines.append("using Godot;")
	lines.append("")
	lines.append("namespace %s" % NAMESPACE)
	lines.append("{")
	lines.append("    /// <summary>")
	lines.append("    /// 配置管理单例。游戏启动时自动加载所有 JSON 配置表。")
	lines.append("    /// 由 JsonToClassGenerator 插件自动生成，请勿手动编辑。")
	lines.append("    /// </summary>")
	lines.append("    public partial class ConfigManager : Node")
	lines.append("    {")
	lines.append("        public static ConfigManager Instance { get; private set; }")
	lines.append("")

	# 为每个类生成 List 和 Dic 属性
	for cs_class_name in class_names:
		var camel := _to_camel_case(cs_class_name)
		lines.append("        /// <summary>%s 配置列表</summary>" % cs_class_name)
		lines.append("        public List<%s> %sList { get; private set; }" % [cs_class_name, camel])
		lines.append("        /// <summary>%s 配置字典（以 ID 为键）</summary>" % cs_class_name)
		lines.append("        public Dictionary<int, %s> %sDic { get; private set; }" % [cs_class_name, camel])
		lines.append("")

	# _Ready 方法：加载所有配置
	lines.append("        public override void _Ready()")
	lines.append("        {")
	lines.append("            if (Instance != null)")
	lines.append("            {")
	lines.append("                GD.PrintErr(\"[ConfigManager] 单例已存在，重复创建！\");")
	lines.append("                QueueFree();")
	lines.append("                return;")
	lines.append("            }")
	lines.append("")
	lines.append("            Instance = this;")
	lines.append("")

	for cs_class_name in class_names:
		var camel := _to_camel_case(cs_class_name)
		var json_name := _to_snake_case(cs_class_name)
		lines.append("            %sList = JsonLoader.LoadToList<%s>(\"%s\");" % [camel, cs_class_name, json_name])
		lines.append("            %sDic = JsonLoader.LoadToDic<%s>(\"%s\");" % [camel, cs_class_name, json_name])
		lines.append("            GD.Print(\"[ConfigManager] %s loaded: List=\" + (%sList?.Count ?? 0) + \", Dic=\" + (%sDic?.Count ?? 0));" % [cs_class_name, camel, camel])
		lines.append("")

	lines.append("            GD.Print(\"[ConfigManager] 所有配置表加载完成\");")
	lines.append("        }")
	lines.append("    }")
	lines.append("}")
	lines.append("")

	var cs_code := "\n".join(lines)

	var output_file := "res://%s/ConfigManager.cs" % CONFIG_MANAGER_DIR
	var fa := FileAccess.open(output_file, FileAccess.WRITE)
	if fa == null:
		return "无法写入文件: %s" % output_file
	fa.store_string(cs_code)
	fa.close()

	return ""


## PascalCase → camelCase
## 例如: PlaceInfo → placeInfo, ItemName → itemName
func _to_camel_case(text: String) -> String:
	if text == "":
		return ""
	return text[0].to_lower() + text.substr(1)


## PascalCase → snake_case
## 例如: PlaceInfo → place_info, ItemName → item_name
func _to_snake_case(text: String) -> String:
	if text == "":
		return ""
	var result := ""
	for i in range(text.length()):
		var ch := text[i]
		if ch == ch.to_upper() and i > 0:
			result += "_" + ch.to_lower()
		else:
			result += ch.to_lower()
	return result
