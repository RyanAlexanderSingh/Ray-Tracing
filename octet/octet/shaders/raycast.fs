//////////////////////////////////////////////////////////////////////////////////////////
//
// raycast shader example
//

// inputs
varying vec3 model_pos_;
uniform vec3 camera_pos;
uniform vec3 sphere_pos;

struct Sphere{
	vec4 position;	
};

//const vec4 spheres[] = { vec4(0, 0, 0, 2.5), vec4(-3, 0, 0, 0.5), vec4(0, 3, 0, 0.5), vec4(0, -3, 0, 0.5) };
const vec4 colours[] = { vec4(1, 0, 0, 1), vec4(0, 1, 0, 1), vec4(0, 0, 1, 1), vec4(0, 1, 1, 1) };

float squared(float f) { return f * f; }

// http://en.wikipedia.org/wiki/Line%E2%80%93sphere_intersection
void main() {
  vec3 ray_start = model_pos_;
  vec3 ray_direction = normalize(model_pos_ - camera_pos);
  float min_d = 1e37;

  const int num_spheres = 3;
  Sphere spheres[num_spheres];

  spheres[0] = Sphere(vec4(0, 0, 0, 2.5));
  spheres[1] = Sphere(vec4(-3, 0, 0, 0.5));
  spheres[2] = Sphere(vec4(0, 3, 0, 0.5));

  spheres[0].position.x = sphere_pos.x;

  for (int i = 0; i != num_spheres; ++i) {
    vec3 omc = ray_start - spheres[i].position.xyz;
    // solve (omc + d * ray_direction)^2 == r^2 for d
    // d^2 * ray_direction^2 + 2 * dot(ray_direction, omc) + omc^2 - r^2 == 0
    float b = dot(ray_direction, omc), c = dot(omc, omc) - squared(spheres[i].position.w);
    if (b*b - c >= 0) {
      float d = -b - sqrt(b*b - c);
      vec3 pos = ray_start + ray_direction * d;
      vec3 normal = normalize(pos - spheres[i].position.xyz);
      if (d < min_d) {
        float rdotn = dot(normal, -ray_direction);
        float specular = rdotn <= 0 ? 0 : pow(rdotn, 10);
        gl_FragColor = colours[i] + vec4(1, 1, 1, 1) * specular;
        min_d = d;
      }
    }
  }

  if (min_d == 1e37) {
    gl_FragColor = vec4(0.3, 0.3, 0.3, 1);
    //discard;
  }
}

