
namespace novel
{
    public static class NovelCustomParameters
    {
        [System.Serializable]
        [NovelParameterAlias("CharacterState")]
        public class CharacterStateParameter : CustomParameter
        {
            public CharacterState state;


            [System.Serializable]
            public class CharacterState
            {
                public int state1;
                public int state2;
                public int state3;
            }
        }
    }
}
