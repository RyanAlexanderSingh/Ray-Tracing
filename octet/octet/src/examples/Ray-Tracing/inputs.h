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
      mat4t &camera_mat = app_scene->get_camera_instance(0)->get_node()->access_nodeToParent();
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

    //we're going to want an init function
    void init(app *app, visual_scene *vs){
      this->the_app = app;
      app_scene = vs;
    }
   };
}

#endif
