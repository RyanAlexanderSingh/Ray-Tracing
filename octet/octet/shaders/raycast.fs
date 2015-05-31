#ifdef GL_ES
precision mediump float;
#endif

//////////////////////////////////////////////////////////////////////////////////////////
//
// raycast in shader example
//

//---------------------------------------------------------
uniform float time;
uniform vec2 resolution;
uniform vec2 mouse;

//---------------------------------------------------------

#define ROTATE false

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
  bool cylinder;
};

struct Plane{
  vec3 normal;
  Material material;
};

struct Wall{
  float radius;
  vec3 position;
  Material material;
};

//---------------------------------------------------------

const float epsilon = 1e-3;
const int iterations = 10;

const float exposure = 1e-2;
const float gamma = 2.2;

const float intensity = 100.0;
const vec3 ambient = vec3(0.6, 0.8, 1.0) * intensity / gamma;

//---------------------------------------------------------

Light light = Light(vec3(1.0) * intensity, normalize(vec3(1.0 + 5.0 * cos(time / 5.0), 4.75, 1.0 + 4.0 * sin(time / 5.0))));
//Light light = Light(vec3(1.0) * intensity, normalize(vec3(-0.0, 0.75, 1.0)));
const Intersect miss = Intersect(0.0, vec3(0.0), Material(vec3(0.0), 0.0, 0.0));

//---------------------------------------------------------

const int num_spheres = 5;
Sphere spheres[num_spheres];

void generateSpheres(){
  spheres[0] = Sphere(3.0, vec3(0.0, 3.0, 0), Material(vec3(0.7, 0.15, 0.125), 1.0, 0.079), false);
  spheres[1] = Sphere(4.0, vec3(8.0, 4.0, 0), Material(vec3(1.0, 0.354,0.725), 0.5, 1.0), false);
  spheres[2] = Sphere(1.0, vec3(3.5, 1.0, 6.0), Material(vec3(1.0, 1.0, 1.0), 0.3, 0.25), false);
  spheres[3] = Sphere(1.0, vec3(-2.5, 1.0, 4.0), Material(vec3(0.2, 0.237, 0.473), 0.8, 0.75), true);
  spheres[4] = Sphere(0.5, vec3(1.0, 1.5 + sin(time), 7.0), Material(vec3(1.0, 1.0, 0.0), 1.0, 0.0), false);
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

Intersect intersect_cylinder(Ray ray, Sphere sphere) {
	vec3 RC;
	float d;
	float t,s;
	vec3 n, D, O;
	float ln;
	float fin, fout;
	
	
	RC = ray.origin - sphere.position;
	n = cross(ray.direction,vec3(0.0,1.0,0.0));
	ln = dot(n,n);
	
	d = abs(dot(RC,n));
	
	if(d<=sphere.radius){
		O = cross(RC,vec3(0.0,1.0,0.0));
		t = -dot(O,n) / ln;
		O = cross(n, vec3(0.0,1.0,0.0));
		s = abs( sqrt(sphere.radius*sphere.radius-d*d));
		
		fin =t-s;
		fout = t+s;
		float len = fout;
		if(fin<-0.)
		{
			if(fout<-0.) return miss;
			else len = fout;
		}
		else if(fout<-0.0){
			len = fin;
		}
		else if(fin<fout)
		{
			len = fin;
		}
		else
		{
			len = fout;
		}
		float h = ray.origin.y+ray.direction.y*len;
		if(h>5.0) return miss;
		if(h<0.0) return miss;
		
		vec3 normal = ray.origin+len*ray.direction - sphere.position;
		normal.y = 0.0;
		normal = normalize(normal);
		return Intersect(len,normal,sphere.material);
	}
	return miss;
}

Intersect intersect(Ray ray, Plane plane) {
  float len = -dot(ray.origin, plane.normal) / dot(ray.direction, plane.normal);
  vec3 col = vec3(0.8, 0.9, 1.0);
  vec3 pos = ray.origin + ray.direction*len;
  float f = mod( floor(1.0*pos.z) + floor(1.0*pos.x), 2.0);
  col = 0.4 + 0.1*f*vec3(1.0);
  return (len < 0.0) ? miss : Intersect(len, plane.normal, Material(col, 1.0, 0.0));
}

Intersect intersect_wall(Ray ray, Wall wall) {
      //we need to check for a negative sqrt
    vec3 oc = wall.position - ray.origin;
    float l = dot(ray.direction, oc);
    float det = pow(l, 2.0) - dot(oc, oc) + pow(wall.radius, 2.0);
    if (det < 0.0) return miss;

    float len = l - sqrt(det);
    if (len < 0.0) len = l + sqrt(det);
    if (len < 0.0) return miss;
    return Intersect(len, (ray.origin + len*ray.direction - wall.position) / wall.radius, wall.material);
}

Intersect trace(Ray ray) {

    Intersect intersection = miss;
    Intersect plane = intersect(ray,  Plane(vec3(0, 1, 0), Material(vec3(0.6, 0.6, 0.6), 1.0, 0.0)));
    if (plane.material.diffuse > 0.0 || plane.material.specular > 0.0) { intersection = plane; }
	
    float distances[num_spheres];
    for(int i=0; i<num_spheres; ++i){
        vec3 dist = spheres[i].position - ray.origin;
        distances[i]= dot(ray.direction, dist);
    }
    for(int i=0; i<num_spheres; ++i)
    {
      for(int j=0; j<num_spheres; ++j) 
      {
        if(distances[j] < distances[i])
        {
          float temp = distances[i];
          distances[i] = distances[j];
          distances[j] = temp;
          Sphere tempsphere = spheres[i];
          spheres[i] = spheres[j];
          spheres[j] = tempsphere;
       }
     }
  }
    for(int i=0; i<num_spheres; ++i)
    {
        Intersect sphere;
	sphere = spheres[i].cylinder ? intersect_cylinder(ray,spheres[i]) : intersect(ray, spheres[i]);
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
  generateSpheres();
	
 vec2 uv = (-1.0 + 2.0 * gl_FragCoord.xy / resolution.xy) / vec2(1.0, resolution.x / resolution.y);
	
  Ray camera_ray;
  // camera  
  float rx = 30.0*cos(6.0*mouse.x);
  float rz = 30.0*sin(6.0*mouse.x);
  camera_ray.origin = vec3( rx, (3.0 * mouse.y) +2.0 , rz );
  vec3 ta = vec3( 0.0, 3.0, 0.0);

  // camera tx
  vec3 cw = normalize( ta-camera_ray.origin );
  vec3 cp = vec3( 0.0, 1.0, 0.0 );
  vec3 cu = normalize( cross(cw, cp) );
  vec3 cv = normalize( cross(cu, cw) );
  camera_ray.direction = normalize( uv.x*cu + uv.y*cv + 2.5*cw );
	
  //use this one for WebGL purposes (debugging)
  Ray ray = ROTATE ? camera_ray : Ray(vec3(3, 2.5, 14.0), normalize(vec3(uv.x, uv.y, -1.0)));
  //use this one for Octet
 // Ray ray = Ray(vec3(ray_pos.x, ray_pos.y, ray_pos.z), normalize(vec3(uv.x, uv.y, -1.0)));

  gl_FragColor = vec4(pow(radiance(ray) * exposure, vec3(1.0 / gamma)), 1.0);
}