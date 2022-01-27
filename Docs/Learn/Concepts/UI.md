# Resizing
## Fixed (`fixed`)
Fixed widget and height based on width and height variables.
## Hug Contents (`hug`)
Based on content size, widget must be an auto layout, forces fill widgets to hug.
## Fill Container (`fill`)
Based on the amount of space not taken up my fixed and hug elements distributed among all other fill widgets.
# Updating
## Hierarchy
```
- auto, fill
	- auto, fill
		- fixed
		- fill
		- min
	- fill
```
