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



        /* initialize plugin */
        private void init()
        {
            print("[autostage] Auto Staging System by mmd (26/06/13)");

            for (int i=0; i<stages.Length; i++)
            {
                stages[i] = new Stage(3000 + i * 1000, 100);
            }
        }



        /* create GUI */
        private void WindowGUI(int windowID)
        {
            GUIStyle mySty = new GUIStyle(GUI.skin.button);
            mySty.normal.textColor = mySty.focused.textColor = Color.white;
            mySty.hover.textColor = mySty.active.textColor = Color.yellow;
            mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
            mySty.padding = new RectOffset(10, 10, 10, 10);

            GUILayout.BeginVertical();
            GUILayout.Label(vessel.GetHeightFromTerrain().ToString("ALT: 0.0"));
            GUILayout.Label("ALT           THR");

            foreach (Stage stage in stages)
            {
                float.TryParse(stage.altitudeTxt, out stage.altitude);
                int.TryParse(stage.throttleTxt, out stage.throttle);
                GUILayout.BeginHorizontal();
                stage.altitudeTxt = GUILayout.TextField(stage.altitudeTxt, GUILayout.Width(60));
                stage.throttleTxt = GUILayout.TextField(stage.throttleTxt, GUILayout.Width(40));
                GUILayout.EndHorizontal();
            }

            run = GUILayout.Toggle(run, "TOGGLE", mySty, GUILayout.ExpandWidth(true));
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));

        }



        /* draw GUI */
        private void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(1, windowPos, WindowGUI, "Auto Stage", GUILayout.MinWidth(100));
        }



        /* called when vessel is placed on the launchpad */
        protected override void onFlightStart()
        {
            init();
            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));
            vessel.OnFlyByWire += flyFunc; /* register fly-by-wire control function */
        }



        /* callback when part is started */
        protected override void onPartStart()
        {
            if ((windowPos.x == 0) && (windowPos.y == 0))
            {
                windowPos = new Rect(Screen.width - 130, 10, 10, 10); /* position the GUI */
            }
        }



        /* callback when part is updated
         * activates stages and changes throttle which is then set by fly-by-wire function
         */
        protected override void onPartUpdate()
        {
            if (stop) stop = false; /* toggle engines back on (assumes that fly() was called by ksp) */

            if (run)
            {
                double curAlt = vessel.GetHeightFromTerrain();

                foreach (Stage stage in stages)
                {
                    if (!stage.staged && Math.Abs(stage.altitude - curAlt) < 30)
                        stop = true;

                    if (!stage.staged && Math.Abs(stage.altitude - curAlt) < 10)
                    {
                        Staging.ActivateNextStage();
                        this.throttle = stage.throttle; /* set the current throttle to value of current stage throttle */
                        stage.staged = true;
                    }
                }
            }

        }



        /* callback when part is disconnected from the ship */
        protected override void onDisconnect()
        {
            vessel.OnFlyByWire -= flyFunc; /* remove the fly-by-wire function */
        }



        /* callback when part is destroyed */
        protected override void onPartDestroy()
        {
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); /* close the GUI */
            vessel.OnFlyByWire -= flyFunc; /* remove the fly-by-wire function */
        }



        /* called every frame and modifies flight controls */
        private void flyFunc(FlightCtrlState fcs)
        {
            if (this.run && this.stop)
                fcs.mainThrottle = 0.0F;
            else if (this.run && !this.stop)
                fcs.mainThrottle = this.throttle / 100.0F;
        }

    }
}
