using System;
using UnityEngine;

namespace OLDD_camera
{
	public static class Styles
	{

        public static GUIStyle guiStyleLabelWhiteNormal;
        public static GUIStyle guiStyleLabelWhiteBold;
        public static GUIStyle guiStyleGreenLabelSmall;
        public static GUIStyle guiStyleGreenLabelStandart;
        public static GUIStyle guiStyleGreenLabelBold;
        public static GUIStyle guiStyleRedLabelNormal;
        public static GUIStyle guiStyleRedLabelBold;
        public static GUIStyle guiStyleRedLabelBoldLarge;

		static Styles()
		{
            guiStyleLabelWhiteNormal = new GUIStyle("label") { fontSize = 13 };
            guiStyleLabelWhiteNormal.normal.textColor = Color.white;

            guiStyleLabelWhiteBold = new GUIStyle("label") { fontSize = 13, fontStyle = FontStyle.Bold };
            guiStyleLabelWhiteBold.normal.textColor = Color.white;

            guiStyleGreenLabelSmall = new GUIStyle("label") { fontSize = 11 };
            guiStyleGreenLabelSmall.normal.textColor = Color.green;

            guiStyleGreenLabelStandart = new GUIStyle(guiStyleGreenLabelSmall) { fontSize = 13 };

            guiStyleGreenLabelBold = new GUIStyle(guiStyleGreenLabelSmall) { fontSize = 15, fontStyle = FontStyle.Bold };
            guiStyleGreenLabelBold.alignment = TextAnchor.MiddleCenter;

            guiStyleRedLabelNormal = new GUIStyle("label") { fontSize = 13, fontStyle = FontStyle.Normal };
            guiStyleRedLabelNormal.normal.textColor = Color.red;
            guiStyleRedLabelNormal.alignment = TextAnchor.MiddleCenter;

            guiStyleRedLabelBold = new GUIStyle(guiStyleRedLabelNormal) { fontStyle = FontStyle.Bold };

            guiStyleRedLabelBoldLarge = new GUIStyle("label") { fontSize = 25, fontStyle = FontStyle.Bold };
            guiStyleRedLabelBoldLarge.normal.textColor = Color.red;
            guiStyleRedLabelBoldLarge.alignment = TextAnchor.MiddleCenter;
        }
	}
}
