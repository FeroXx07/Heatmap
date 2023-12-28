using Server;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/User Data")]
    public class UserSO : ScriptableObject
    {
        [Header("User Info")] 
        public string Name = "Enter a Name";
        public string Sex = "Enter your Sex";
        public int Age = 20;
        public string Country = "Enter your Country";
    }
}
