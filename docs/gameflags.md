# GameFlagsView
## Booleans
- **LowGravity** - lowers gravity.
- **NoArmor** - disables armor.
- **QuickSwitch** - enables holstering of weapons before firing is complete.
- **MeleeOnly** - disables all weapons except melee.
- **DefenseBonus** - adds the defense bonus feature, allowing armorweight to take effect.

All of the previously listed bools return their values through the [IsFlagSet](#Functions) function, and set their values through the SetFlag function.

## Functions
- **private bool IsFlagSet(GameFlags f)** - checks the instanced state and returns whether or not it has found the input GameFlag.
- **public static bool IsFlagSet(GameFlags flag, int state)** - similar to the private instanced version of this function, except this takes the state as input and checks the input state for the specified GameFlag.  
- **public int GetFlags()** - returns the state of all current GameFlags.
