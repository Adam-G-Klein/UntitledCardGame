
void SelectValue(float Dummy, out float OutValue) 
{
    #if _ENUM_INDEX_1
        OutValue = 1.0;
    #elif _ENUM_INDEX_2
        OutValue = 2.0;
    #elif _ENUM_INDEX_3
        OutValue = 3.0;
    #elif _ENUM_INDEX_4
        OutValue = 4.0;
    #else
        OutValue = 0.0; // Default if no valid index is selected
    #endif
}
