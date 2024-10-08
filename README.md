https://github.com/user-attachments/assets/f29f70e1-a75f-4278-972b-babb8c7a3857
 
WIP. Under Active development. Follow [@Singtaa](https://x.com/Singtaa) for updates.

The [OneJS](https://onejs.com) bindings are available at https://github.com/Singtaa/onejs-spark2d. Currently, the library contains the following compute shaders:

1. fbm: Fractal Brownian Motion for generating noise
2. sdfield: Signed Distance Field for generating 2D shapes
3. sdgen: Jump Flood based SDF generation
4. maop: Math Operations for common math operations
5. trans: Transformations for rotating, scaling, and translating textures
6. blur: Gaussian Blur for blurring textures
7. grad: Gradient for generating multi-color linear gradients
8. dye: for applying multi-color gradients to greyscale textures

`sdfield` contains almost all the 2D shapes on [Inigo Quilez's page](https://iquilezles.org/articles/distfunctions2d/).

`maop` contains the following ops:

```hlsl
// Basic
#define OP_ADD 0
#define OP_SUBTRACT 1
#define OP_MULTIPLY 2
#define OP_DIVIDE 3
#define OP_POW 4
#define OP_SQRT 5

// Range
#define OP_CLAMP 16
#define OP_FRACTION 17
#define OP_MAXIMUM 18
#define OP_MINIMUM 19
#define OP_ONE_MINUS 20
#define OP_RANDOM_RANGE 21
#define OP_REMAP 22
#define OP_SATURATE 23

// Advanced
#define OP_ABSOLUTE 32
#define OP_EXPONENTIAL 33
#define OP_LENGTH 34
#define OP_LOG 35
#define OP_MODULO 36
#define OP_NEGATE 37
#define OP_NORMALIZE 38
#define OP_POSTERIZE 39
#define OP_RECIPROCAL 40
#define OP_RECIPROCAL_SQRT 41
```

Besides the compute shaders, the library also contains a Bursted (faster) version of CatlikeCoding's [SDF Texture Generator](https://catlikecoding.com/sdf-toolkit/docs/texture-generator/).

The goal is to add many more useful 2D algorithms and compute shaders to this library. A 2D particle system is also in the works. 
