# artistic-image-effect-dotnet

ideas:

## Algorithmic canvas pattern ideas

### Geometric and symmetry

- **Concentric circles**: Rings with varying radii, stroke weights, and colour palettes; add jitter for hand-drawn feel.

- **Spirographs (hypotrochoids/epitrochoids)**: Parametric curves; e.g. x = (R−r)cos t + d cos((R−r)/r t), y = (R−r)sin t − d sin((R−r)/r t).

- **Rose curves (rhodonea)**: Polar equation *r = cos(kθ)*; vary k for petal count and symmetry.

- **Lissajous figures**: *x = A sin(a t + δ), y = B sin(b t)*; elegant oscillatory loops.

- **Phyllotaxis spiral**: Place points at angle *θ = n·137.5°*, radius *r = c√n*; yields sunflower-like arrangements.

- **Star polygons and polygon spirals**: {n/k} star sequences; rotate/scale per step for layered rosettes.

### Fractals and recursive structures

- **Mandelbrot and Julia sets**: Iteration of *z ← z² + c*; map iterations to colour for rich textures.

- **Sierpinski triangle/carpet**: Recursive removal; strong geometric minimalism.

- **Koch snowflake**: Edge subdivision with equilateral bumps; crisp snowflake contours.

- **Barnsley fern (IFS)**: Affine transforms with probabilities; organic fern silhouette.

- **Pythagoras tree**: Recursive squares forming a branching structure; vary angle for diversity.

### Noise and fields

- **Perlin/Simplex noise landscapes**: Heightmaps, contours, or shaded textures; domain scaling for detail.

- **Domain-warped noise**: Warp coordinates with another noise field to create complex marbling.

- **Flow fields with particle advection**: Move thousands of particles along a vector field; leave trails for silky patterns.

- **Worley (cellular) noise**: Cell-based textures; distance metrics produce stone/foam effects.

- **Turbulence-warped grids**: Distort regular grids with noise; yields woven or liquid-like lattices.

### Tessellations and partitions

- **Voronoi diagrams**: Partition space by nearest seed; colour by region metrics for variation.

- **Delaunay triangulation**: Triangulate points; shade by triangle area, angle, or gradient for faceted art.

- **Penrose tiling**: Aperiodic kite/dart or rhomb tilings; non-repeating symmetry.

- **Truchet tiles**: Simple tile motifs randomly rotated; emergent maze-like patterns.

- **Hex/triangular tilings**: Regular tessellations with palette gradients or per-cell textures.

### Op-art and optical effects

- **Moiré overlays**: Interference from layered lines or dots; rotate/scale layers for dynamic effects.

- **Checkerboard distortions**: Warp a grid by sine lenses or radial functions for “wavy” illusions.

- **Halftone dot fields**: Dot size encodes intensity; experiment with hexagonal dot packing.

- **Isometric cubes**: Repeating 3D illusion via rhombus cells; alternate shading for depth.

- **Radial stripes and spirals**: Angular bands with sinus modulation for hypnotic results.

### Organic and simulation-based

- **Reaction–diffusion (Gray–Scott)**: Simulated chemistry producing animal-like patterns.

- **Diffusion-limited aggregation (DLA)**: Particles stick to a cluster; fractal coral/frost structures.

- **Boids trails**: Flocking agents leave motion traces; interplay of cohesion/alignment/separation.

- **Sandpile model**: Cellular toppling yields intricate contour maps.

- **Cellular automata (Game of Life, Lenia)**: Emergent motifs from simple local rules.

### Line and point systems

- **Stippling (Poisson disk sampling)**: Evenly distributed dots; density maps add shading.

- **TSP art**: Single continuous path through points approximates a portrait.

- **Contour lines (marching squares/triangles)**: Iso-lines from scalar fields; cartographic aesthetics.

- **Hatching along gradients**: Lines follow vector fields; vary thickness and spacing for tone.

- **Random walkers with constraints**: Agents meander, avoid overlap, and form lace-like webs.