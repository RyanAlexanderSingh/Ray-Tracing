//////////////////////////////////////////////////////////////////////////////////////////
//
// raycast shader example (currently using struct framework off glsl project)
//

uniform float time;
uniform vec2 resolution;
uniform vec2 mouse;
uniform vec3 ray_pos;

struct Ray {
  vec3 origin;
  vec3 direction;
};

struct Light {
  vec3 color;
  vec3 direction;
};

struct Material{
  vec3 color;
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
  Material material;
};

struct Plane{
  vec3 normal;
  Material material;
};

float squared(float f) { return f * f; }

const float epsilon = 1e-3;
const float iterations = 18;

const float exposure = 1e-2;
const float gamma = 2.2;

const float intensity = 100.0;
const vec3 ambient = vec3(0.6, 0.8, 1.0) * intensity / gamma;

 Light light = Light(vec3(1.0) * intensity, normalize(vec3(1.0 + 5.0 * cos(time / 5.0), 4.75, 1.0 + 4.0 * sin(time / 5.0))));

const Intersect miss = Intersect(0.0, vec3(0.0), Material(vec3(0.0), 0.0, 0.0));

const int num_spheres = 5;
Sphere spheres[num_spheres];

void generateShapes(){

  spheres[0] = Sphere(3.0, vec3(0.0, 3.0, 0), Material(vec3(0.7, 0.15, 0.125), 1.0, 0.079));
  spheres[1] = Sphere(4.0, vec3(8.0, 4, 0), Material(vec3(1.0, 0.354,0.725), 0.5, 1.0));
  spheres[2] = Sphere(1.0, vec3(3.5, 1.0, 6.0), Material(vec3(1.0, 1.0, 1.0), 0.3, 0.25));
  spheres[3] = Sphere(1.0, vec3(-1.5, 1.0, 4.0), Material(vec3(0.2, 0.237, 0.473), 0.8, 0.75));
  spheres[4] = Sphere(0.5, vec3(1.0, 0.5, 7.0), Material(vec3(1.0, 1.0, 0.0), 1.0, 0.0));
	
}

Intersect intersect(Ray ray, Sphere sphere) {
    // Check for a Negative Square Root
    vec3 oc = sphere.position - ray.origin;
    float l = dot(ray.direction, oc);
    float det = pow(l, 2.0) - dot(oc, oc) + pow(sphere.radius, 2.0);
    if (det < 0.0) return miss;

    // Find the Closer of Two Solutions
             float len = l - sqrt(det);
    if (len < 0.0) len = l + sqrt(det);
    if (len < 0.0) return miss;
    return Intersect(len, (ray.origin + len*ray.direction - sphere.position) / sphere.radius, sphere.material);
}


Intersect intersect(Ray ray, Plane plane) {
    float len = -dot(ray.origin, plane.normal) / dot(ray.direction, plane.normal);
    return (len < 0.0) ? miss : Intersect(len, plane.normal, plane.material);
}

Intersect trace(Ray ray) {

    Intersect intersection = miss;
    Intersect plane = intersect(ray, Plane(vec3(0, 1, 0), Material(vec3(0.6, 0.6, 0.6), 1.0, 0.0)));
    if (plane.material.diffuse > 0.0 || plane.material.specular > 0.0) { intersection = plane; }
    for (int i = 0; i < num_spheres; i++) {
        Intersect sphere = intersect(ray, spheres[i]);
        if (sphere.material.diffuse > 0.0 || sphere.material.specular > 0.0)
            intersection = sphere;
    }
    return intersection;
}

vec3 radiance(Ray ray) {
    vec3 color, fresnel;
    vec3 mask = vec3(1.0);
    for (int i = 0; i <= iterations; ++i) {
        Intersect hit = trace(ray);

        if (hit.material.diffuse > 0.0 || hit.material.specular > 0.0) {

            vec3 r0 = hit.material.color.rgb * hit.material.specular;
            float hv = clamp(dot(hit.normal, -ray.direction), 0.0, 1.0);
            fresnel = r0 + (1.0 - r0) * pow(1.0 - hv, 5.0); mask *= fresnel;


            if (trace(Ray(ray.origin + hit.len * ray.direction + epsilon * light.direction, light.direction)) == miss) {
                color += clamp(dot(hit.normal, light.direction), 0.0, 1.0) * light.color
                       * hit.material.color.rgb * hit.material.diffuse  // Add Diffuse
                       * (1.0 - fresnel) * mask / fresnel;         // Subtract Specular
            }

            vec3 reflection = reflect(ray.direction, hit.normal);
            ray = Ray(ray.origin + hit.len * ray.direction + epsilon * reflection, reflection);

        } 
        else {
            vec3 spotlight = vec3(1e6) * pow(abs(dot(ray.direction, light.direction)), 600.0);
            color += mask * (ambient + spotlight); break;
        }
    }
    return color;
}


void main() {
  //Implementation of new code
  //http://en.wikipedia.org/wiki/Line%E2%80%93sphere_intersection
  generateShapes();

 vec2 uv = (-1.0 + 2.0 * gl_FragCoord.xy / resolution.xy) / vec2(1.0, resolution.x / resolution.y);
  //use this one for WEBGL purposes (debugging)
  //Ray ray = Ray(vec3(0.0, 2, 10.0), normalize(vec3(uv.x, uv.y, -1.0)));
  //use this one for Octet
  Ray ray = Ray(vec3(ray_pos.x, ray_pos.y, ray_pos.z), normalize(vec3(uv.x, uv.y, -1.0)));

  gl_FragColor = vec4(pow(radiance(ray) * exposure, vec3(1.0 / gamma)), 1.0);
}

