using UnityEngine;
using UnityEditor;
using Game.Gameplay.Character.Ladder;

namespace Game.Editor
{
    // Menu: Game / Ladder Setup / Create Placeholder Ladder
    // Builds a simple primitive-only ladder (2 rails + rungs) with a trigger volume sized
    // to the climbable height, for testing ClimbState before real art exists.
    public static class LadderSetupWizard
    {
        private const float Height       = 3f;
        private const float Width        = 0.6f;
        private const float RailThickness = 0.06f;
        private const float RungSpacing  = 0.3f;
        private const float RungThickness = 0.05f;

        [MenuItem("Game/Ladder Setup/Create Placeholder Ladder")]
        public static void CreatePlaceholderLadder()
        {
            var existing = GameObject.Find("Ladder (Placeholder)");
            if (existing != null)
            {
                Debug.Log("[LadderSetup] Found existing Ladder (Placeholder) GO.");
                Selection.activeGameObject = existing;
                return;
            }

            var root = new GameObject("Ladder (Placeholder)");
            root.transform.position = new Vector3(0f, 0f, 10f);

            CreateRail(root.transform, "Rail_Left",  -Width * 0.5f);
            CreateRail(root.transform, "Rail_Right",   Width * 0.5f);

            int rungCount = Mathf.FloorToInt(Height / RungSpacing);
            for (int i = 0; i <= rungCount; i++)
                CreateRung(root.transform, i, i * RungSpacing);

            // Trigger volume defines the climbable range directly — ClimbState reads its
            // Top/BottomY (via LadderZone) to know when to step the character off.
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, Height * 0.5f, 0.15f);
            col.size   = new Vector3(Width + 0.4f, Height, 0.6f);

            root.AddComponent<LadderZone>();

            // Solid landing platform at the top — ClimbState steps the character ~1m forward
            // (along +Z, this GameObject's forward) on dismount. Surface sits a bit below the
            // dismount height (default ClimbDismountMargin=0.15 + a small up-nudge in
            // ClimbState) so they land ON it via a short natural drop instead of clipping in.
            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Platform";
            platform.transform.SetParent(root.transform, false);
            platform.transform.localPosition = new Vector3(0f, Height - 0.5f, 1.3f);
            platform.transform.localScale    = new Vector3(2f, 0.2f, 2f);

            Debug.Log("[LadderSetup] Created placeholder ladder at " + root.transform.position +
                      ". Replace the primitive rails/rungs with real art later — LadderZone/BoxCollider stay put.");
            Selection.activeGameObject = root;
        }

        private static void CreateRail(Transform parent, string name, float xOffset)
        {
            var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = name;
            rail.transform.SetParent(parent, false);
            rail.transform.localPosition = new Vector3(xOffset, Height * 0.5f, 0f);
            rail.transform.localScale    = new Vector3(RailThickness, Height, RailThickness);
            Object.DestroyImmediate(rail.GetComponent<BoxCollider>()); // parent trigger handles detection
        }

        private static void CreateRung(Transform parent, int index, float y)
        {
            var rung = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rung.name = "Rung_" + index;
            rung.transform.SetParent(parent, false);
            rung.transform.localPosition = new Vector3(0f, y, 0f);
            rung.transform.localScale    = new Vector3(Width, RungThickness, RungThickness);
            Object.DestroyImmediate(rung.GetComponent<BoxCollider>());
        }
    }
}
