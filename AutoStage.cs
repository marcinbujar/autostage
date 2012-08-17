using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace autostage
{
    public class AutoStage : Part
    {
        protected Stage[] stages = new Stage[5];
        protected bool run = false;
        protected bool stop = false;
        protected int throttle = 100;
        protected Rect windowPos;


        private void init()
        {
            print("Auto Staging System by mmd [build 17.08.2012]");

            stages[0] = new Stage(3000, 100);
            stages[1] = new Stage(6000, 100);
            stages[2] = new Stage(7000, 100);
            stages[3] = new Stage(8000, 100);
            stages[4] = new Stage(9000, 100);
        }


        private void WindowGUI(int windowID)
        {
            GUIStyle mySty = new GUIStyle(GUI.skin.button);
            mySty.normal.textColor = mySty.focused.textColor = Color.white;
            mySty.hover.textColor = mySty.active.textColor = Color.yellow;
            mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
            mySty.padding = new RectOffset(10, 10, 10, 10);

            GUILayout.BeginVertical();
            GUILayout.Label("ALT          THR");

            foreach (Stage s in stages)
            {
                float.TryParse(s.altitudeTxt, out s.altitude);
                int.TryParse(s.throttleTxt, out s.throttle);
                GUILayout.BeginHorizontal();
                s.altitudeTxt = GUILayout.TextField(s.altitudeTxt, GUILayout.Width(60));
                s.throttleTxt = GUILayout.TextField(s.throttleTxt, GUILayout.Width(40));
                GUILayout.EndHorizontal();
            }
            
            run = GUILayout.Toggle(run, "TOGGLE", mySty, GUILayout.ExpandWidth(true));

            GUILayout.EndVertical();

            //DragWindow makes the window draggable. The Rect specifies which part of the window it can by dragged by, and is 
            //clipped to the actual boundary of the window. You can also pass no argument at all and then the window can by
            //dragged by any part of it. Make sure the DragWindow command is AFTER all your other GUI input stuff, or else
            //it may "cover up" your controls and make them stop responding to the mouse.
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

        }


        private void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(1, windowPos, WindowGUI, "Auto Stage", GUILayout.MinWidth(100));
        }


        protected override void onFlightStart()  //Called when vessel is placed on the launchpad
        {
            init();

            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI)); //start the GUI

            //at the beginning of the flight, register fly-by-wire control function that will be called repeatedly
            FlightInputHandler.OnFlyByWire += new FlightInputHandler.FlightInputCallback(fly);
        }


        protected override void onPartStart()
        {
            if ((windowPos.x == 0) && (windowPos.y == 0)) //windowPos is used to position the GUI window
            {
                windowPos = new Rect(Screen.width - 130, 10, 10, 10);
            }
        }


        protected override void onPartUpdate()
        {
            if (stop) stop = false; //toggle engines back on (assumes that fly() was called by ksp

            if (run) //do stuff only if we are running
            {
                double curAlt = FlightGlobals.getAltitudeAtPos(FlightGlobals.ship_position);

                foreach (Stage s in stages)
                {
                    if (!s.staged && Math.Abs(s.altitude - curAlt) < 20)
                        stop = true;

                    if (!s.staged && Math.Abs(s.altitude - curAlt) < 10)
                    {
                        Staging.ActivateNextStage();
                        this.throttle = s.throttle; //set the current throttle to value of current stage throttle
                        s.staged = true;
                    }
                }
            }

        }

        
        protected override void onDisconnect()
        {
            //remove the fly-by-wire function when we get disconnected from the ship:
            FlightInputHandler.OnFlyByWire -= new FlightInputHandler.FlightInputCallback(fly);
        }


        protected override void onPartDestroy()
        {
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
            FlightInputHandler.OnFlyByWire -= new FlightInputHandler.FlightInputCallback(fly);
        }


        //this function gets called every frame or something and gives access to the flight controls
        private void fly(FlightCtrlState s)
        {
            //s.yaw = -0.2F;  //set yaw input to 20% left
            //s.pitch += 0.3F; //set pitch input to whatever the player has input + 30%
            //s.roll = 0.5F;   //set roll to 50% (either clockwise or counterclockwise, try it and find out)
            //s.mainThrottle = 0.8F; //set throttle to 80%

            //the range of yaw, pitch, and roll is -1.0F to 1.0F, and the throttle goes from 0.0F to 1.0F.
            //if your code might violate that it's probably a good idea to clamp the inputs, e.g.:
            //s.roll = Mathf.Clamp(s.roll, -1.0F, +1.0F);
            if (this.run && this.stop) s.mainThrottle = 0.0F;
            else if (this.run && !this.stop) s.mainThrottle = this.throttle / 100.0F;
        }

    }
}
