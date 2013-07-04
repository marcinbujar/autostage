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
        protected bool systemRun = false;
        protected bool enginesOn = true;
        protected int throttle = 100;
        protected Rect windowPos;



        /* initialize plugin */
        private void init()
        {
            print("[autostage] Auto Staging System by mmd (03/07/13)");

            for (int i=0; i<stages.Length; i++)
            {
                stages[i] = new Stage(3000 + i * 1000, 100);
            }
        }



        /* GUI callback */
        private void WindowGUI(int windowID)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.normal.textColor = style.focused.textColor = Color.white;
            style.hover.textColor = style.active.textColor = Color.yellow;
            style.onNormal.textColor = style.onFocused.textColor = style.onHover.textColor = style.onActive.textColor = Color.green;
            style.padding = new RectOffset(10, 10, 10, 10);

            GUILayout.BeginVertical();
            GUILayout.Label(vessel.GetHeightFromTerrain().ToString("ALT 0m"));
            GUILayout.Label("ALT             THR");

            foreach (Stage stage in stages)
            {
                float.TryParse(stage.altitudeTxt, out stage.altitude);
                int.TryParse(stage.throttleTxt, out stage.throttle);
                GUILayout.BeginHorizontal();
                stage.altitudeTxt = GUILayout.TextField(stage.altitudeTxt, GUILayout.Width(60));
                stage.throttleTxt = GUILayout.TextField(stage.throttleTxt, GUILayout.Width(40));
                GUILayout.EndHorizontal();
            }

            systemRun = GUILayout.Toggle(systemRun, "TOGGLE", style, GUILayout.ExpandWidth(true));
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
            this.init();
            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));
            vessel.OnFlyByWire += this.flyFunc; /* register fly-by-wire control function */
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
            if (!this.enginesOn) this.enginesOn = true; /* toggle engines back on (assumes that fly() was called by ksp) */

            if (this.systemRun)
            {
                double curAlt = vessel.GetHeightFromTerrain();

                foreach (Stage stage in this.stages)
                {
                    if (!stage.staged && Math.Abs(stage.altitude - curAlt) < 30)
                        this.enginesOn = false;

                    if (!stage.staged && Math.Abs(stage.altitude - curAlt) < 10)
                    {
                        Staging.ActivateNextStage();
                        this.throttle = stage.throttle; /* set the current throttle to value of current stage throttle */
                        stage.staged = true;
						print("[autostage] Stage Activated");
                    }
                }
            }

        }



        /* callback when part is disconnected from the ship */
        protected override void onDisconnect()
        {
            vessel.OnFlyByWire -= this.flyFunc; /* remove the fly-by-wire function */
        }



        /* callback when part is destroyed */
        protected override void onPartDestroy()
        {
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(this.drawGUI)); /* close the GUI */
            vessel.OnFlyByWire -= this.flyFunc; /* remove the fly-by-wire function */
        }



        /* called every frame and modifies flight controls */
        private void flyFunc(FlightCtrlState fcs)
        {
            if (this.systemRun) {
                if (!this.enginesOn)
                    fcs.mainThrottle = 0.0F;
                else
                    fcs.mainThrottle = this.throttle / 100.0F;
            }
        }

    }
}
