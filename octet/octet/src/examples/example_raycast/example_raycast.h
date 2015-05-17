////////////////////////////////////////////////////////////////////////////////
//
// (C) Andy Thomason 2012-2014
//
// Modular Framework for OpenGLES2 rendering on multiple platforms.
//
namespace octet {
  /// Scene containing a box with octet.
  class example_raycast : public app {
    // scene for drawing box
    ref<visual_scene> app_scene;

    ref<material> custom_mat;

    ref<param_uniform> camera_pos;
    ref<param_uniform> rand_pos;

    ref<camera_instance> camera;

    int i = 0;

  public:
    /// this is called when we construct the class before everything is initialised.
    example_raycast(int argc, char **argv) : app(argc, argv) {
    }

    /// this is called once OpenGL is initialized
    void app_init() {
      app_scene = new visual_scene();
      app_scene->create_default_camera_and_lights();

      param_shader *shader = new param_shader("shaders/default.vs", "shaders/raycast.fs");
      custom_mat = new material(vec4(1, 1, 1, 1), shader);
      atom_t atom_camera_pos = app_utils::get_atom("camera_pos");
      vec3 random_position = vec3(0, 0, 0);
      atom_t atom_rand_pos = app_utils::get_atom("sphere_pos");
      rand_pos = custom_mat->add_uniform(&random_position, atom_rand_pos, GL_FLOAT_VEC3, 1, param::stage_fragment);

      vec3 val(0, 0, 0);
      camera_pos = custom_mat->add_uniform(&val, atom_camera_pos, GL_FLOAT_VEC3, 1, param::stage_fragment);

      mesh_box *box = new mesh_box(vec3(5));
      scene_node *node = new scene_node();
      app_scene->add_child(node);
      app_scene->add_mesh_instance(new mesh_instance(node, box, custom_mat));
    }

    /// this is called to draw the world
    void draw_world(int x, int y, int w, int h) {
      int vx = 0, vy = 0;
      get_viewport_size(vx, vy);
      app_scene->begin_render(vx, vy);

      scene_node *camera_node = app_scene->get_camera_instance(0)->get_node();
      scene_node *box_node = app_scene->get_mesh_instance(0)->get_node();

      // camera position in model space
      vec3 pos = camera_node->get_position();
      //char tmp[256]; printf("%s\n", pos.toString(tmp, 256));
      custom_mat->set_uniform(camera_pos, &pos, sizeof(pos));

      if (is_key_down('W')) {
        camera_node->translate(vec3(0, 0, 1));
        ++i;
      }
      if (is_key_down('S'))
      {
        camera_node->translate(vec3(0, 0, -1));
        --i;
      }
      if (is_key_down('A')) camera_node->translate(vec3(1, 0, 0));
      if (is_key_down('D')) camera_node->translate(vec3(-1, 0, 0));

      vec3 random_position = vec3(i, 0, 0);
      custom_mat->set_uniform(rand_pos, &random_position, sizeof(random_position));

      // update matrices. assume 30 fps.
      app_scene->update(1.0f / 30);

      // draw the scene
      app_scene->render((float)vx / vy);
    }
  };
}
