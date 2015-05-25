///////////////////////////////////////f////////////////////////////////////////
//
// (C) Ryan Singh 2015
//
// Based off Chapter 1. Effective Water Simulation from Physical Models http://http.developer.nvidia.com/GPUGems/gpugems_ch01.html
// Using the Gerstner wave function 
//

#ifndef INPUTS_H_INCLUDED
#define INPUTS_H_INCLUDED

namespace octet{

  class inputs : public resource{

    app *the_app;
    ref<visual_scene> app_scene;
    ref<camera_instance> camera;

  public:
    inputs(){}

    void mouse_inputs(){
      //mouse control using x and y pos of mouse
      int x, y;
      the_app->get_mouse_pos(x, y);
      //AntTweakBar stuff end
      int vx, vy;
      the_app->get_viewport_size(vx, vy);

      mat4t modelToWorld;
      camera = app_scene->get_camera_instance(0);
      mat4t &camera_mat = camera->get_node()->access_nodeToParent();
      modelToWorld.loadIdentity();
      modelToWorld[3] = vec4(camera_mat.w().x(), camera_mat.w().y(), camera_mat.w().z(), 1);
      modelToWorld.rotateY((float)-x*2.0f);
      if (vy / 2 - y < 70 && vy / 2 - y > -70)
        modelToWorld.rotateX((float)vy / 2 - y);
      if (vy / 2 - y >= 70)
        modelToWorld.rotateX(70);
      if (vy / 2 - y <= -70)
        modelToWorld.rotateX(-70);
      camera_mat = modelToWorld;//apply to the node
    }

    void keyboard_inputs(){
      if (the_app->is_key_down('W')){
        camera->get_node()->translate(vec3(0, 0, -1));
      }
      if (the_app->is_key_down('S')){
        camera->get_node()->translate(vec3(0, 0, 1));
      }
      if (the_app->is_key_down('A')){
        camera->get_node()->translate(vec3(-1, 0, 0));
      }
      if (the_app->is_key_down('D')){
        camera->get_node()->translate(vec3(1, 0, 0));
      }
      if (the_app->is_key_down(key_esc)){
        exit(1);
      }
    }

    //we're going to want an init function
    void init(app *app, visual_scene *vs){
      this->the_app = app;
      app_scene = vs;
    }

    void update(){
      //mouse_inputs();
      //keyboard_inputs();
    }
   };
}

#endif
