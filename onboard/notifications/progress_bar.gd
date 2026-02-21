extends ProgressBar

func _ready() -> void:
    value = 0

func _process(delta: float) -> void:
    value += delta
    if value >= max_value:
        value = 0
