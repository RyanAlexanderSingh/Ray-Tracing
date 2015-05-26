////////////////////////////////////////////////////////////////////////////////
//
// (C) Andy Thomason 2012-2014 -- Octet Framework
//
// Ryan Singh - Raytracer project for Advanced Mathmematics (29/05/2015)
//

#include <chrono>
#include <ctime>

namespace octet {
  /// Scene for drawing a simple raytracer with a set of spheres and a plane.
  class RayTracing : public app {

    std::chrono::time_point<std::chrono::system_clock> start_time;

    // scene for drawing box
    ref<visual_scene> app_scene;

    ref<material> raytracer;

    ref<camera_instance> camera;

    //uniform params
    ref<param_uniform> uniform_mouse;
    ref<param_uniform> uniform_resolution;
    ref<param_uniform> uniform_time;
    ref<param_uniform> uniform_ray_pos;

    vec3 ray_position;

    inputs inputs;

  public:
    /// this is called when we construct the class before everything is initialised.
    RayTracing(int argc, char **argv) : app(argc, argv) {
    }

    ///We need to update the raytracer with uniforms such as time.
    void update_uniforms(){
      scene_node *camera_node = camera->get_node();

      int x, y;
      get_mouse_pos(x, y);
      vec2 mouse_pos = vec2(x, y);
      raytracer->set_uniform(uniform_mouse, &mouse_pos, sizeof(mouse_pos));

      //Update the time for the sphere movemenets
      std::chrono::time_point<std::chrono::system_clock> now = std::chrono::system_clock::now();
      std::chrono::duration<float> time_elapsed = now - start_time;
      float time = time_elapsed.count();
      raytracer->set_uniform(uniform_time, &time, sizeof(time));

      raytracer->set_uniform(uniform_ray_pos, &ray_position, sizeof(ray_position));
    }

    /// this is called once OpenGL is initialized
    void app_init() {
      app_scene = new visual_scene();
      app_scene->create_default_camera_and_lights();
      camera = app_scene->get_camera_instance(0);
      camera->get_node()->translate(vec3(0, 0, -30));

      start_time = std::chrono::system_clock::now();

      inputs.init(this, app_scene, &ray_position);

      param_shader *shader = new param_shader("shaders/default.vs", "shaders/raycast.fs");
      raytracer = new material(vec4(1, 1, 1, 1), shader);

      int vx = 0, vy = 0;
      get_viewport_size(vx, vy);
      vec2 viewport(vx, vy);
      atom_t atom_resolution = app_utils::get_atom("resolution");
      uniform_resolution = raytracer->add_uniform(&viewport, atom_resolution, GL_FLOAT_VEC2, 1, param::stage_fragment);

      int x, y;
      get_mouse_pos(x, y);
      vec2 mouse_pos = vec2(x, y);
      atom_t atom_camera_pos = app_utils::get_atom("mouse");
      uniform_mouse = raytracer->add_uniform(&mouse_pos, atom_camera_pos, GL_FLOAT_VEC2, 1, param::stage_fragment);

      float time = 0;
      atom_t atom_time = app_utils::get_atom("time");
      uniform_time = raytracer->add_uniform(&time, atom_time, GL_FLOAT, 1, param::stage_fragment);

      ray_position = vec3(0.0f, 2.5f, 12.0f);
      atom_t atom_ray_pos = app_utils::get_atom("ray_pos");
      uniform_ray_pos = raytracer->add_uniform(&ray_position, atom_ray_pos, GL_FLOAT_VEC3, 1, param::stage_fragment);

      mesh_box *box = new mesh_box(vec3(10));
      scene_node *node = new scene_node();
      app_scene->add_child(node);
      app_scene->add_mesh_instance(new mesh_instance(node, box, raytracer));
    }

    /// this is called to draw the world
    void draw_world(int x, int y, int w, int h) {
      int vx = 0, vy = 0;
      get_viewport_size(vx, vy);
      app_scene->begin_render(vx, vy);

      inputs.update();
      update_uniforms();

      // update matrices. assume 30 fps.
      app_scene->update(1.0f / 30);

      // draw the scene
      app_scene->render((float)vx / vy);
    }
  };
}
