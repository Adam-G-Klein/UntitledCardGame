// This exists to allow the FX Experience custom editor to find all custom
// clips meant to work with the FX Experience script
using UnityEngine;

public interface IFXExperienceClip {
    ExposedReference<FXExperience> GetExposedReference();
}