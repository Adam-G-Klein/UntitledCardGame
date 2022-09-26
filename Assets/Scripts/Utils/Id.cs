using System;

public static class Id {
    public static string newGuid(){
        return Guid.NewGuid().ToString();
    }
}