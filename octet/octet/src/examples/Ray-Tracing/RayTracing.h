////////////////////////////////////////////////////////////////////////////////
//
// (C) Andy Thomason 2012-2014
//
// Modular Framework for OpenGLES2 rendering on multiple platforms.
//
namespace octet {
  /// Scene containing a box with octet.
  class RayTracing : public app {
    // scene for drawing box
    ref<visual_scene> app_scene;

    ref<material> raytracer;
    ref<camera_instance> camera;

    //uniform params
    ref<param_uniform> camera_pos;
    ref<param_uniform> sphere_pos;

    inputs inputs; 

    int i = 0;

  public:
    /// this is called when we construct the class before everything is initialised.
    RayTracing(int argc, char **argv) : app(argc, argv) {
    }

    /// this is called once OpenGL is initialized
    void app_init() {
      app_scene =  new visual_scene();
      app_scene->create_default_camera_and_lights();

      inputs.init(this, app_scene);

      param_shader *shader = new param_shader("shaders/default.vs", "shaders/raycast.fs");
      raytracer = new material(vec4(1, 1, 1, 1), shader);
      vec3 sphere_position(0, 0, 0);
      atom_t atom_sphere_pos = app_utils::get_atom("sphere_pos");
      sphere_pos = raytracer->add_uniform(&sphere_position, atom_sphere_pos, GL_FLOAT_VEC3, 1, param::stage_fragment);

      vec3 val(0, 0, 0);
      atom_t atom_camera_pos = app_utils::get_atom("camera_pos");
      camera_pos = raytracer->add_uniform(&val, atom_camera_pos, GL_FLOAT_VEC3, 1, param::stage_fragment);

      mesh_box *box = new mesh_box(vec3(5));
      scene_node *node = new scene_node();
      app_scene->add_child(node);
      app_scene->add_mesh_instance(new mesh_instance(node, box, raytracer));
    }

    /// this is called to draw the world
    void draw_world(int x, int y, int w, int h) {
      int vx = 0, vy = 0;
      get_viewport_size(vx, vy);
      app_scene->begin_render(vx, vy);

      scene_node *camera_node = app_scene->get_camera_instance(0)->get_node();
      // camera position in model space
      vec3 pos = camera_node->get_position();
      //char tmp[256]; printf("%s\n", pos.toString(tmp, 256));
      raytracer->set_uniform(camera_pos, &pos, sizeof(pos));

      //inputs.mouse_inputs();

      // update matrices. assume 30 fps.
      app_scene->update(1.0f/30);

      // draw the scene
      app_scene->render((float)vx / vy);
    }
  };
}
