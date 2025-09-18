using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public enum GFGInputAction
{
    UP,
    DOWN,
    LEFT,
    RIGHT,
    SELECT,
    BACK,
    END_TURN,
    OPEN_COMPANION_1_DRAW, // Deprecated
    OPEN_COMPANION_2_DRAW, // Deprecated
    OPEN_COMPANION_3_DRAW, // Deprecated
    OPEN_COMPANION_4_DRAW, // Deprecated
    OPEN_COMPANION_5_DRAW, // Deprecated
    NONE, // used for the controller mapping of the stick being in the center of its range
    CUTSCENE_SKIP,
    SECONDARY_UP,
    SECONDARY_DOWN,
    SECONDARY_RIGHT,
    SECONDARY_LEFT,
    SELECT_DOWN,
    SELECT_UP,
    VIEW_DECK,
    VIEW_DISCARD,
    SELL_COMPANION,
    OPTIONS,
    OPEN_MULTI_DECK_VIEW,
    MULTI_DECK_VIEW_TAB_LEFT,
    MULTI_DECK_VIEW_TAB_RIGHT,
    MULTI_DECK_VIEW_SECTION_LEFT,
    MULTI_DECK_VIEW_SECTION_RIGHT
}

public enum InputMethod {
    Mouse,
    Controller,
    Keyboard
}