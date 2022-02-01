﻿using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speckle.ConnectorDynamo.Functions
{
  /// <summary>
  /// In memory cache to have the receive node return data that is not coming from the ASTfunction call
  /// </summary>
  [IsVisibleInDynamoLibrary(false)]
  public static class InMemoryCache
  {
    private static Dictionary<string, Dictionary<string, object>> _cache = new Dictionary<string, Dictionary<string, object>>();

    public static Dictionary<string, object> Get(string id)
    {
      if (_cache.ContainsKey(id))
        return _cache[id];
      return null;
    }

    public static void Set(string id, Dictionary<string, object> dic)
    {
      _cache[id] = dic;
    }
  }
}
