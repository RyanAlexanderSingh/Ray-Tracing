////////////////////////////////////////////////////////////////////////////////
//
// (C) Andy Thomason 2012-2014
//
// Modular Framework for OpenGLES2 rendering on multiple platforms.
//

#include <chrono>
#include <ctime>

namespace octet {
  /// Scene containing a box with octet.
  class RayTracing : public app {
    
    std::chrono::time_point<std::chrono::system_clock> start_time;
    
    // scene for drawing box
    ref<visual_scene> app_scene;

    ref<material> raytracer;
    ref<camera_instance> camera;

    //uniform params
    ref<param_uniform> camera_pos;
    ref<param_uniform> uniform_resolution;
    ref<param_uniform> uniform_time;

    inputs inputs; 

  public:
    /// this is called when we construct the class before everything is initialised.
    RayTracing(int argc, char **argv) : app(argc, argv) {
    }

    void update_spheres(){
      scene_node *camera_node = camera->get_node();
      vec3 pos = camera_node->get_position();
      raytracer->set_uniform(camera_pos, &pos, sizeof(pos));

      //Update the time for the sphere movemenets
      std::chrono::time_point<std::chrono::system_clock> now = std::chrono::system_clock::now();
      std::chrono::duration<float> time_elapsed = now - start_time;
      float time = time_elapsed.count();

      raytracer->set_uniform(uniform_time, &time, sizeof(time));
    }

    /// this is called once OpenGL is initialized
    void app_init() {
      app_scene =  new visual_scene();
      app_scene->create_default_camera_and_lights();

      camera = app_scene->get_camera_instance(0);

      start_time = std::chrono::system_clock::now();

      inputs.init(this, app_scene);

      param_shader *shader = new param_shader("shaders/default.vs", "shaders/raycast.fs");
      raytracer = new material(vec4(1, 1, 1, 1), shader);
    
      int vx = 0, vy = 0;
      get_viewport_size(vx, vy);
      vec2 viewport(vx, vy);
      atom_t atom_resolution = app_utils::get_atom("resolution");
      uniform_resolution = raytracer->add_uniform(&viewport, atom_resolution, GL_FLOAT_VEC2, 1, param::stage_fragment);

      vec3 val(0);
      atom_t atom_camera_pos = app_utils::get_atom("camera_pos");
      camera_pos = raytracer->add_uniform(&val, atom_camera_pos, GL_FLOAT_VEC3, 1, param::stage_fragment);

      float time = 0;
      atom_t atom_time = app_utils::get_atom("time");
      uniform_time = raytracer->add_uniform(&time, atom_time, GL_FLOAT, 1, param::stage_fragment);
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
      update_spheres();

      // update matrices. assume 30 fps.
      app_scene->update(1.0f/30);

      // draw the scene
      app_scene->render((float)vx / vy);
    }
  };
}
