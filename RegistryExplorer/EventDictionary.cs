
namespace RegistryExplorer.Collections
{


    class ItemEventArgs<TKey, TValue>
        : System.EventArgs
    {
        public System.Collections.Generic.KeyValuePair<TKey, TValue> Item
        {
            get;
            set;
        }

        public ItemEventArgs(TKey key, TValue value)
        {
            Item = new System.Collections.Generic.KeyValuePair<TKey, TValue>(key, value);
        }
    }

    class EventDictionary<TKey, TValue> 
        : System.Collections.Generic.Dictionary<TKey, TValue>
    {
        public event System.EventHandler<ItemEventArgs<TKey, TValue>> ItemAdded;
        public event System.EventHandler<ItemEventArgs<TKey, TValue>> ItemRemoved;

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            if (ItemAdded != null)
                ItemAdded(this, new ItemEventArgs<TKey, TValue>(key, value));
        }

        public new bool Remove(TKey key)
        {
            TValue value = base[key];
            bool result = base.Remove(key);
            if (result && ItemRemoved != null)
                ItemRemoved(this, new ItemEventArgs<TKey, TValue>(key, value));
            return result;
        }

        public new TValue this[TKey key]
        {
            get
            {
                return base[key];
            }
            set
            {
                bool exists = base.ContainsKey(key);
                base[key] = value;
                if (!exists && ItemAdded != null)
                    ItemAdded(this, new ItemEventArgs<TKey, TValue>(key, value));
            }
        }

    }


}
