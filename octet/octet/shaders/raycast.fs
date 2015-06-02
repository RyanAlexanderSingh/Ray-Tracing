#ifdef GL_ES
precision mediump float;
#endif

//////////////////////////////////////////////////////////////////////////////////////////
//
// raycast in shader example
//

#define ROTATE true
#define OCTET false

//---------------------------------------------------------

uniform float time;
uniform vec2 resolution;
uniform vec2 mouse;
uniform vec3 ray_pos;

//---------------------------------------------------------

struct Ray {
  vec3 origin;
  vec3 direction;
};

struct Light {
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
  Material material;
  bool cylinder;
  float bh;
};

struct Plane{
  vec3 normal;
  Material material;
};
//---------------------------------------------------------


const float epsilon = 1e-3;
const int iterations = 16; //iterative approach opposed to typical recursive ray casting algorithms.

// a miss is defined as unedited pixel. (nothing is drawn)
const Intersect miss = Intersect(0.0, vec3(0.0), Material(vec3(0.0), 0.0, 0.0)); 
//---------------------------------------------------------

//magic numbers for the light have been sourced by many GLSL projects which use ray tracing.
const float gamma = 2.2;
const float intensity = 100.0;
const vec3 ambient = vec3(0.6, 0.8, 1.0) * intensity / gamma;

//There are two lights here, current using moving one but feel free to comment out the static light.
Light light = Light(vec3(1.0) * intensity, normalize(vec3(1.0 + 5.0 * cos(time / 5.0), 4.75, 1.0 + 4.0 * sin(time / 5.0))));
//Light light = Light(vec3(1.0) * intensity, normalize(vec3(-1.0, 0.75, 1.0)));

//---------------------------------------------------------

const int num_spheres = 7;
Sphere spheres[num_spheres];

///Generation of the spheres. The boolean control simply defines if the sphere geometry is more like a cylinder with an intersecting sphere.
void generateSpheres(){
  spheres[0] = Sphere(3.0, vec3(0.0, 3.0, 0), Material(vec3(0.7, 0.15, 0.125), 1.0, 0.079), false, 0.);
  spheres[1] = Sphere(4.0, vec3(8.0, 4.0, 0), Material(vec3(0.0, 0.9354,0.25), 0.5, 1.0), false, 0.);
  spheres[5] = Sphere(2.0, vec3(10.0, 7.0, 1.0), Material(vec3(0.0, 0.9354,0.25), 0.5, 1.0), false, 0.);
  spheres[6] = Sphere(2.0, vec3(6.0, 7.0, 1.0), Material(vec3(0.0, 0.9354,0.25), 0.5, 1.0), false, 0.);
  spheres[2] = Sphere(1.0, vec3(3.5, 1.0, 6.0), Material(vec3(1.0, 1.0, 1.0), 0.3, 0.25), false, 0.);
  spheres[3] = Sphere(1.0, vec3(-2.5, 5.0, 4.0), Material(vec3(0.2, 0.237, 0.473), 0.8, 0.75), true, 1.);
  spheres[4] = Sphere(0.5, vec3(1.0, 1.5, 7.0), Material(vec3(1.0, 0.318, 0.1), 1.0, 0.0), true, 0.);
}

//---------------------------------------------------------

Intersect intersect(Ray ray, Sphere sphere) {
    //we need to check for a negative sqrt
    vec3 oc = sphere.position - ray.origin;
    float l = dot(ray.direction, oc);
    float det = pow(l, 2.0) - dot(oc, oc) + pow(sphere.radius, 2.0);
    if (det < 0.0) return miss;

    float len = l - sqrt(det);
    if (len < 0.0) return miss;
    return Intersect(len, (ray.origin + len*ray.direction - sphere.position) / sphere.radius, sphere.material);
}

///Checking for a cylinder intersection is slightly more difficult than a sphere. 
Intersect intersect_cylinder(Ray ray, Sphere sphere) {
	
  vec3 raycast = ray.origin - sphere.position;
  vec3 sphere_axis = vec3(0.0, 1.0, 0.0);
  vec3 n = cross(ray.direction,sphere_axis);
  float ln = length(n);
	
	// Parallel? (?)
  if((ln<0.)&&(ln>-0.)) return miss;
	 
	n = normalize(n);
	float d = abs(dot(raycast, n));
	
	if(d <= sphere.radius) {
         	vec3 origin = cross(raycast, sphere_axis);
		float t = -dot(origin, n) / ln;
		origin = cross(n, sphere_axis);
		float s = abs( sqrt(sphere.radius *sphere.radius - d*d) / dot (ray.direction, origin));
		
		float fin =t-s;
		float fout = t+s;
		float len;
   		 //we need to check if these are non zeros.
		if(fin < -0.)
		{
			if(fout < -0.) return miss;
			else len = fout;
		}
		else if(fout < -0.) len = fin;
		else if(fin < fout) len = fin;
		else len = fout;

		float hit = ray.origin.y+ray.direction.y*len;
		//to generate cylinders/pill shapes, we have to draw the tops as spheres if the ray is out of bounds of the y pos (a miss)
		if(hit > sphere.position.y) return intersect(ray, sphere); 
		if(hit < sphere.bh) {sphere.position.y = 1.0; return intersect(ray, sphere);}
		
		vec3 normal = ray.origin+len*ray.direction - sphere.position;
		normal.y = 0.0;
		normal = normalize(normal);
		return Intersect(len,normal,sphere.material);
	}
	return miss;
}

Intersect intersect(Ray ray, Plane plane) {
  float len = -dot(ray.origin, plane.normal) / dot(ray.direction, plane.normal);
  vec3 pos = ray.origin + ray.direction*len;
  //col is the material (pattern) of the floor plane.
  vec3 col = vec3(0.8, 0.9, 1.0);
  float f = mod( floor(1.0*pos.z) + floor(1.0*pos.x), 2.0);
  col = 0.4 + 0.1*f*vec3(1.0);
  return (len < 0.0) ? miss : Intersect(len, plane.normal, Material(col, 1.0, 0.0));
}

Intersect trace(Ray ray) {
  bool first = true;
  Intersect intersection = miss;
  Intersect plane = intersect(ray,  Plane(vec3(0, 1, 0), Material(vec3(0.6, 0.6, 0.6), 1.0, 0.0)));
  if (plane.material.diffuse > 0.0 || plane.material.specular > 0.0) { intersection = plane; }
  //only calculate the nearest hits to the ray when we are rotating the camera around
  for(int i=0;i<num_spheres;++i) {
    Intersect sphere = spheres[i].cylinder ? intersect_cylinder(ray,spheres[i]) : intersect(ray, spheres[i]);
    if (sphere.len > 0. && (first || intersection.len > sphere.len)){
        intersection = sphere;
	first = false;
    }
  }
  return intersection;
}

vec3 radiance(Ray ray) {
   
  vec3 colour, fresnel;
  vec3 mask = vec3(1.0);
  for (int i = 0; i <= iterations; ++i) {
    Intersect hit = trace(ray);
    if (hit.material.diffuse > 0.0 || hit.material.specular > 0.0) {
      vec3 r0 = hit.material.colour.rgb * hit.material.specular;
      float hv = clamp(dot(hit.normal, -ray.direction), 0.0, 1.0);
      fresnel = r0 + (1.0 - r0) * pow(1.0 - hv, 5.0); mask *= fresnel;

      if (trace(Ray(ray.origin + hit.len * ray.direction + epsilon * light.direction, light.direction)) == miss) {
        colour += clamp(dot(hit.normal, light.direction), 0.0, 1.0) * light.colour * hit.material.colour.rgb * hit.material.diffuse * (1.0 - fresnel) * mask / fresnel;
      }
      vec3 reflection = reflect(ray.direction, hit.normal);
      ray = Ray(ray.origin + hit.len * ray.direction + epsilon * reflection, reflection);
    } 
    else {
    vec3 spotlight = vec3(1e6) * pow(abs(dot(ray.direction, light.direction)), 600.0);
    colour += mask * (ambient + spotlight); break;
    }
  }
  return colour;
}


void main() {
  //http://en.wikipedia.org/wiki/Line%E2%80%93sphere_intersection
  generateSpheres();
	
 vec2 uv = (-1.0 + 2.0 * gl_FragCoord.xy / resolution.xy) / vec2(1.0, resolution.x / resolution.y);
	
  Ray camera_ray;
  // camera  
  float t = time * 0.5;
  float ray_x = 30.0*cos(t+6.0);
  float ray_z = 30.0*sin(t+6.0);
  camera_ray.origin = vec3( ray_x, (3.0) +2.0 , ray_z );
  vec3 ta = vec3( 0.0, 3.0, 0.0);

  // camera tx
  vec3 cam_w = normalize( ta-camera_ray.origin );
  vec3 cam_pos = vec3( 0.0, 1.0, 0.0 );
  vec3 cu = normalize( cross(cam_w, cam_pos) );
  vec3 cv = normalize( cross(cu, cam_w) );
  camera_ray.direction = normalize( uv.x*cu + uv.y*cv + 2.5*cam_w );
	
  vec3 ray_position = OCTET ? ray_pos : vec3(3, 2.5, 12.5);
  //use this one for WebGL purposes (debugging)
  Ray ray = ROTATE ? camera_ray : Ray(ray_position, normalize(vec3(uv.x, uv.y, -1.0)));
	
  const float exposure = 1e-2;
  gl_FragColor = vec4(pow(radiance(ray) * exposure, vec3(1.0 / gamma)), 1.0);
}