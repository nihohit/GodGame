using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static class Constants {
  public const float MaxHeight = 100;
  public const float MinHeight = -100;
  public const float TileLength = 10;
}

public enum TileUpdateType {
    LowerRaise,
    Flatten
}

public enum TileUpdateDirection {
    Up,
    Down
}

