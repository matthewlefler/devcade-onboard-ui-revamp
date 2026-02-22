extends Control

var window_position: Vector2i

func _ready() -> void:
    get_window().always_on_top = true;
    get_window().visible = true;
    
    var window_size: Vector2i = DisplayServer.screen_get_size()
    
    var viewport_size: Vector2 = get_viewport_rect().size
    
    window_position = Vector2i(window_size.x - viewport_size.x - ((window_size.x - viewport_size.x) / 2), window_size.y - viewport_size.y)
    get_window().position = window_position

func _notification(what: int) -> void:
    if what == NOTIFICATION_WM_POSITION_CHANGED:
        get_window().position = window_position
    if what == NOTIFICATION_WM_WINDOW_FOCUS_OUT:
        get_window().always_on_top = true;
        get_window().visible = true;
