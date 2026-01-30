// using Extensions.UtilityAI;
// using Extensions.UtilityAI.ConsiderationBases;
// using UnityEngine;
// using UnityEditor;
//
// namespace UtilityAI {
//     [CustomEditor(typeof(EnemyStateMachine))]
//     public class BrainEditor : Editor {
//         void OnEnable() {
//             this.RequiresConstantRepaint();
//         }
//             
//         public override void OnInspectorGUI() {
//             base.OnInspectorGUI(); // Draw the default inspector
//
//             var t = (EnemyStateMachine) target;
//             AIBrain<EnemyAIContextKey> brain = t.AIBrain;
//
//             if (Application.isPlaying) {
//                 AIAction<EnemyAIContextKey> chosenAction = GetChosenAction(brain);
//
//                 if (chosenAction != null) {
//                     EditorGUILayout.LabelField($"Current Chosen Action: {chosenAction.name}", EditorStyles.boldLabel);
//                 }
//                 
//                 EditorGUILayout.Space();
//                 EditorGUILayout.LabelField("Actions/Considerations", EditorStyles.boldLabel);
//
//
//                 foreach (AIAction<EnemyAIContextKey> action in brain.User.GetActions()) {
//                     float utility = action.CalculateUtility(brain.Context);
//                     EditorGUILayout.LabelField($"Action: {action.name}, Utility: {utility:F2}");
//
//                     // Draw the single consideration for the action
//                     DrawConsideration(action.consideration, brain.Context, 1);
//                 }
//             } else {
//                 EditorGUILayout.HelpBox("Enter Play mode to view utility values.", MessageType.Info);
//             }
//         }
//
//         private void DrawConsideration(Consideration consideration, Context<EnemyAIContextKey> context, int indentLevel) {
//             EditorGUI.indentLevel = indentLevel;
//
//             if (consideration is CompositeConsideration compositeConsideration) {
//                 
//             } else {
//                 float value = consideration.Evaluate(context);
//                 EditorGUILayout.LabelField($"Consideration: {consideration.name}, Value: {value:F2}");
//             }
//
//             EditorGUI.indentLevel = indentLevel - 1; // Reset indentation after drawing
//         }
//
//         private AIAction<EnemyAIContextKey> GetChosenAction(AIBrain<EnemyAIContextKey> brain) {
//             float highestUtility = float.MinValue;
//             AIAction<EnemyAIContextKey> chosenAction = null;
//
//             foreach (var action in brain.User.GetActions()) {
//                 float utility = action.CalculateUtility(brain.Context);
//                 if (utility > highestUtility) {
//                     highestUtility = utility;
//                     chosenAction = action;
//                 }
//             }
//
//             return chosenAction;
//         }
//     }
// }