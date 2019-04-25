using System;

namespace Assets.Scripts.Base {
  using System.Reflection;

  /// <summary>
  /// Manages the single instance of a class.
  /// </summary>
  /// <typeparam name="T">Type of the singleton class.</typeparam>
  public static class Singleton<T>
      where T : class {
    /// <summary>
    /// The dummy object used for locking.
    /// </summary>
    private static readonly object s_lockObj;

    /// <summary>
    /// The single instance of the target class.
    /// </summary>
    /// <remarks>
    /// The volatile keyword makes sure to remove any compiler optimization that could make concurrent
    /// threads reach a race condition with the double-checked lock pattern used in the Instance property.
    /// See http://www.bluebytesoftware.com/blog/PermaLink,guid,543d89ad-8d57-4a51-b7c9-a821e3992bf6.aspx
    /// </remarks>
    private static volatile T s_instance;

    /// <summary>
    /// Type-initializer to prevent type to be marked with beforefieldinit.
    /// </summary>
    /// <remarks>
    /// This simply makes sure that static fields initialization occurs
    /// when Instance is called the first time and not before.
    /// </remarks>
    static Singleton() {
      s_lockObj = new object();
    }

    /// <summary>
    /// Gets the single instance of the class.
    /// </summary>
    public static T Instance {
      get {
        if (s_instance != null) {
          return s_instance;
        }

        lock (s_lockObj) {
          if (s_instance != null) {
            return s_instance;
          }

          // Binding flags exclude public and static constructors.
          ConstructorInfo constructor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);

          // Also exclude internal constructors.
          if (constructor == null || constructor.IsAssembly) {
            throw new Exception(string.Format("A private or protected constructor is missing for '{0}'.", typeof(T).Name));
          }

          s_instance = constructor.Invoke(null) as T;
        }

        return s_instance;
      }
    }
  }
}