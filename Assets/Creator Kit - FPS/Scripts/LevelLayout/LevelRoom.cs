using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class LevelRoom : MonoBehaviour
{
   public Transform[] Exits;

   [HideInInspector]
   public LevelRoom[] ExitDestination;

   [HideInInspector]
   public LevelLayout Owner;

   /// <summary>
   /// Called by the editor script that place room to initialize the ExitUsed array to match Exits
   /// </summary>
   public void Placed(LevelLayout layoutOwner)
   {
      Owner = layoutOwner;
      ExitDestination = new LevelRoom[Exits.Length];
   }
#if UNITY_EDITOR
   public void  Removed()
   {   
      if (ExitDestination != null)
      {
         for (int i = 0; i < ExitDestination.Length; ++i)
         {
            if (ExitDestination[i] != null)
            {
               SerializedObject otherObj = new SerializedObject(ExitDestination[i]);
               var connectorProp = otherObj.FindProperty(nameof(ExitDestination));

               for (int k = 0; k < connectorProp.arraySize; ++k)
               {
                  var prop = connectorProp.GetArrayElementAtIndex(k);

                  if (prop.objectReferenceValue == this)
                  {
                     prop.objectReferenceValue = null;
                     prop.serializedObject.ApplyModifiedProperties();
                  }
               }
            }
         }
      }

      if (Owner != null && !Owner.Destroyed)
      {
            SerializedObject ownerObject = new SerializedObject(Owner);
            
            var piecesProp = ownerObject.FindProperty(nameof(Owner.rooms));

            for (int i = 0; i < piecesProp.arraySize; ++i)
            {
               var prop = piecesProp.GetArrayElementAtIndex(i);

               if (prop.objectReferenceValue == this)
               {
                  piecesProp.DeleteArrayElementAtIndex(i);
                  piecesProp.DeleteArrayElementAtIndex(i);
                  break;
               }
            }
            
            ownerObject.ApplyModifiedProperties();
      }
   }
#endif
}