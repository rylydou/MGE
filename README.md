# MGE

The Mangrove Game Engine

Heavily based on [Foster](https://github.com/NoelFB/Foster)

## Supported Platforms _(only 64 bit)_

- Windows
- Linux
- MacOSX

## Supported Frameworks

### Windowing

- SDL
- GLFW

### Graphics

- OpenGL
- Vulkan _\*WIP\*_

### Audio

- OpenAL _\*WIP\*_

## Fonts

- [Inter](https://github.com/rsms/inter) - Normal Font
- [Iosevka](https://github.com/be5invis/Iosevka) - Code Font

## Dependencies

- [FastNoiseLite](https://github.com/Auburn/FastNoiseLite) - Implemented in [MGE.Noise](MGE/Common/Noise.cs)
- [StbImageSharp](https://github.com/StbSharp/StbImageSharp) - Image loading

# MGE Source Code
We will try to put these `README.md` files in each module/libraries explain everything.

# Modules & Libraries

## Level 0: Completely independent modules
- [MEML](MEML/): Libaries for reading and writing meml files, the json-like markup language used in MGE.
- [Mathf](Mathf/): Provides math and linear algebra utilities.

## Level 1: The Core
- [MGE](MGE/) (MEML, Mathf): The core engine.

## Level 2: Platform implementations
Provides platform implementations, of which there are 3:
- System: For creating a window and getting user input.
- Graphics: For handling render passes provided by the engine.
- Audio: For playing audio.

Everything here is at least dependent on MGE.

[See the README for more details](Platforms/)

## Level 3: Editor & other tooling
- [Editor](/Editor) (MGE): The MGE Editor
