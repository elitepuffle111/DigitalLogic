using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UniParams : MonoBehaviour
{


    public ChipType chipType = new ChipType();
    public Hashtable chipTypeIdx = new Hashtable();

    public ChipClass chipClass = new ChipClass();
    public Hashtable chipClassIdx = new Hashtable();

    public MoveType moveType = new MoveType();
    public Hashtable moveTypeIdx = new Hashtable();

    // Start is called before the first frame update
    void Start()
    {
        populateChipTypes();
        populateChipClasses();
        populateMoveTypes();

    }


    void populateChipClasses()
    {
        chipClassIdx.Add("gate", chipClass.GATE);
        chipClassIdx.Add("primative", chipClass.PRIMATIVE);
        chipClassIdx.Add("triprimative", chipClass.TRIPRIMATIVE);
        chipClassIdx.Add("axis", chipClass.AXIS);
        chipClassIdx.Add("valve", chipClass.VALVE);
        chipClassIdx.Add("triswitch", chipClass.TRISWITCH);
        chipClassIdx.Add("thumbwheel", chipClass.THUMBWHEEL);
        chipClassIdx.Add("flipflop", chipClass.FLIPFLOP);
        chipClassIdx.Add("display", chipClass.DISPLAY);
        chipClassIdx.Add("keyboard", chipClass.KEYBOARD);
        chipClassIdx.Add("footpedal", chipClass.FOOTPEDAL);

    }

    void populateChipTypes()
    {
        chipTypeIdx.Add("and", chipType.AND);
        chipTypeIdx.Add("nand", chipType.NAND);
        chipTypeIdx.Add("or", chipType.OR);
        chipTypeIdx.Add("xor", chipType.XOR);
        chipTypeIdx.Add("nor", chipType.NOR);
        chipTypeIdx.Add("xnor", chipType.XNOR);
        chipTypeIdx.Add("not", chipType.NOT);
        chipTypeIdx.Add("triand", chipType.TRIAND);
        chipTypeIdx.Add("buffer", chipType.BUFFER);

        chipTypeIdx.Add("switch", chipType.SWITCH);
        chipTypeIdx.Add("dial", chipType.DIAL);
        chipTypeIdx.Add("slide", chipType.SLIDE);
        chipTypeIdx.Add("updn", chipType.UPDN);
        chipTypeIdx.Add("lamp", chipType.LAMP);

        chipTypeIdx.Add("dekatron", chipType.DEKATRON);
        chipTypeIdx.Add("unidekatron", chipType.UNDEKATRON);
        chipTypeIdx.Add("nixi", chipType.NIXI);
        chipTypeIdx.Add("digdisplay", chipType.DIGDISPLAY);

        chipTypeIdx.Add("dff", chipType.DFF);

        chipTypeIdx.Add("thumbwheel", chipType.THUMBWHEEL);
        chipTypeIdx.Add("scorewheel", chipType.SCOREWHEEL);
        chipTypeIdx.Add("fan", chipType.FAN);
        chipTypeIdx.Add("relay", chipType.RELAY);

        chipTypeIdx.Add("flipper", chipType.FLIPPER);

        chipTypeIdx.Add("keyboard", chipType.KEYBOARD);
        chipTypeIdx.Add("footpedal", chipType.FOOTPEDAL);

    }

    void populateMoveTypes()
    {
        moveTypeIdx.Add(0, moveType.rotRight);
        moveTypeIdx.Add(1, moveType.rotUp);
        moveTypeIdx.Add(2, moveType.rotLeft);
        moveTypeIdx.Add(3, moveType.pushDown);
        moveTypeIdx.Add(4, moveType.pushLeft);
        moveTypeIdx.Add(5, moveType.rotDown);
        moveTypeIdx.Add(6, moveType.rotLeft);
    }


    [System.Serializable]
    public struct ChipType
    {
        public string AND, NAND, OR, XOR, NOR, XNOR, NOT, TRIAND, BUFFER;
        public string SWITCH, DIAL, SLIDE, UPDN, LAMP;
        public string DEKATRON, UNDEKATRON, NIXI, DIGDISPLAY;
        public string DFF;
        public string THUMBWHEEL, SCOREWHEEL, FAN, RELAY;
        public string FLIPPER;
        public string KEYBOARD, FOOTPEDAL;

    }

    [System.Serializable]
    public struct ChipClass
    {
        public string GATE,PRIMATIVE, TRIPRIMATIVE, AXIS, VALVE, TRISWITCH, THUMBWHEEL, FLIPFLOP, DISPLAY, KEYBOARD, FOOTPEDAL;
    }

    [System.Serializable]
    public struct MoveType
    {
        public int rotFwd, rotUp, rotLeft, pushDown, pushLeft, rotDown, rotRight;
    }

}
