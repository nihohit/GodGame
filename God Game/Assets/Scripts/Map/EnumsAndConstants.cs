using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static class Constants {
  public const float MaxHeight = 100;
  public const float MinHeight = -100;
  public const float SizeOfTile = 10;
}

public enum InteractionMode {
  LowerRaiseTile,
  FlattenTile,
  AddTree
}

public enum TileUpdateDirection {
  Up,
  Down
}

