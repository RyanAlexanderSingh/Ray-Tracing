//////////////////////////////////////////////////////////////////////////////////////////
//
// raycast shader example
//

///wtf is prophet666
// inputs
varying vec3 model_pos_;
uniform vec3 camera_pos;


uniform float time;
uniform vec2 resolution;

struct Ray{
  vec3 origin;
  vec3 direction;
}; 

struct Light{
  vec3 colour;
  vec3 direction;
};

struct Material{
vec3 colour;
float diffuse;
float specular;
};

struct Intersect {
float len;
vec3 normal;
Material material;
};

struct Sphere{
	float radius;
  vec3 position;
  vec4 colour; //replace with material later
};

struct Plane{
  vec3 normal;
  Material material;
};

float squared(float f) { return f * f; }

const float exposure = 1e-2;
const float gamma = 2.2;

const float intensity = 100.0f;
const vec3 ambient = vec3(0.6, 0.8, 1.0) * intensity / gamma;

Light light = Light(vec3(1.0) * intensity, normalize(vec3(-1.0, 0.75, 1.0))); 

// http://en.wikipedia.org/wiki/Line%E2%80%93sphere_intersection
void main() {
  //Implementation of new code

  vec2 uv = gl_FragCoord.xy / resolution.xy - vec2(0.5);
       uv.x *= resolution.x / resolution.y;

  const int num_spheres = 3;
  Sphere spheres[num_spheres];

  spheres[0] = Sphere(2.0, vec3(-4.0, 3.0 + sin(time), 0), vec4(1, 0, 0, 1));
  spheres[1] = Sphere(3.0, vec3(4.0 + cos(time), 3.0, 0), vec4(0, 1, 0, 1));
  spheres[2] = Sphere(1.0, vec3(0.5, 1.0, 6.0), vec4(0, 1, 1, 1));

  Ray ray = Ray(vec3(0, 2.5, 12.0), normalize(vec3(uv.x, uv.y, -1.0)));

  //Andys example code
  vec3 ray_start = model_pos_;
  vec3 ray_direction = normalize(model_pos_ - camera_pos);
  float min_d = 1e37;

  for (int i = 0; i != num_spheres; ++i) {
    vec3 omc = ray_start - spheres[i].position.xyz;
    // solve (omc + d * ray_direction)^2 == r^2 for d
    // d^2 * ray_direction^2 + 2 * dot(ray_direction, omc) + omc^2 - r^2 == 0
    float b = dot(ray_direction, omc), c = dot(omc, omc) - squared(spheres[i].radius);
    if (b*b - c >= 0) {
      float d = -b - sqrt(b*b - c);
      vec3 pos = ray_start + ray_direction * d;
      vec3 normal = normalize(pos - spheres[i].position.xyz);
      if (d < min_d) {
        float rdotn = dot(normal, -ray_direction);
        float specular = rdotn <= 0 ? 0 : pow(rdotn, 10);
        gl_FragColor = spheres[i].colour + vec4(1, 1, 1, 1) * specular;
        min_d = d;
      }
    }
  }

  if (min_d == 1e37) {
    gl_FragColor = vec4(0.0, 0., 0.1, 0);
    //discard;
  }
}

