using System;
using System.Collections;

namespace ManifestManagerLib
{
  public class ArrayListHelperBase
  {
    protected ArrayListHelperBase() { }
    protected static int GetListCount(ArrayList list)
    {
      return (list != null) ? list.Count : 0;
    }

    protected static int GetListCapacity(ArrayList list)
    {
      return (list != null) ? list.Capacity : 0;
    }

    protected static void SetListCapacity(ref ArrayList list, int capacity)
    {
      if (capacity > 0)
        if (list != null)
        {
          if (capacity > list.Capacity)
            list.Capacity = capacity;
        }
        else
          list = new ArrayList(capacity);
    }
  }
}
