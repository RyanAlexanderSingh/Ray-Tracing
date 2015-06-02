///////////////////////////////////////f////////////////////////////////////////
//
// (C) Ryan Singh 2015
//
//

#ifndef INPUTS_H_INCLUDED
#define INPUTS_H_INCLUDED

namespace octet{

  class inputs : public resource{

    app *the_app;
    ref<visual_scene> app_scene;
    ref<camera_instance> camera;

    vec3 *ray_position;

  public:
    inputs(){}
    
    ///Key controls move the rays origin position when enabled in raycast.fs 
    void keyboard_inputs(){
      if (the_app->is_key_down('W')){
        --ray_position->z();
      }
      if (the_app->is_key_down('S')){
        ++ray_position->z();
      }
      if (the_app->is_key_down('A')){
        --ray_position->x();
      }
      if (the_app->is_key_down('D')){
        ++ray_position->x();
      }
      if (the_app->is_key_down(key_up)){
        ++ray_position->y();
      }
      if (the_app->is_key_down(key_down)){
        --ray_position->y();
      }
      if (the_app->is_key_down(key_esc)){
        exit(1);
      }
    }

    //we're going to want an init function
    void init(app *app, visual_scene *vs, vec3 *ray_pos){
      this->the_app = app;
      app_scene = vs;
      ray_position = ray_pos;
    }

    void update(){
      keyboard_inputs();
    }
   };
}

#endif
