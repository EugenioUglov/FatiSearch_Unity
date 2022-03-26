using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageService : MonoBehaviour
{
    [HideInInspector] public string PageState;

    public class PageStateEnum
    {
        public const string SearchPage = "SearchPage";
        public const string ActionBlockCreator = "ActionBlockCreator";
        public const string ActionBlockModifier = "ActionBlockModifier";
    }
}
