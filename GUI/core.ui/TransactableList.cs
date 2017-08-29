using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.ui
{
    public enum UITransactionOp
    {
        InsertAt,
        Add,
        AddRange,
        RemoveAt,
        RemoveRange,
        SetAt,
        Clear,
        Checkpoint
    }
    public class UITransaction
    {
        public UITransactionOp op;
        public int i1;
        public int i2;
        public object[] elems;
        public IUITransactable context;
    }
    public class UITransactionManager
    {
        List<UITransaction> trans = new List<UITransaction>();
        int pos=0;

        public UITransactionManager()
        {
            Enabled = true;
        }
        public void Transact(IUITransactable context, UITransactionOp op, object[] elems)
        {
            if (!Enabled)
                return;
            checkRestart();
            UITransaction tr = new UITransaction();
            tr.op=op;
            tr.elems = elems;
            tr.context=context;
            trans.Add(tr);
        }
        public void Transact(IUITransactable context, UITransactionOp op, int i1, object[] elems)
        {
            if (!Enabled)
                return;
            checkRestart();
            UITransaction tr = new UITransaction();
            tr.op = op;
            tr.i1 = i1;
            tr.elems = elems;
            tr.context=context;
            trans.Add(tr);
        }
        public void Transact(IUITransactable context, UITransactionOp op, int i1, int i2, object[] elems)
        {
            if (!Enabled)
                return;
            checkRestart();
            UITransaction tr = new UITransaction();
            tr.op = op;
            tr.i1 = i1;
            tr.i2 = i2;
            tr.elems = elems;
            tr.context=context;
            trans.Add(tr);
        }

        private void checkRestart()
        {
            if(pos!=0)
            {
                trans.RemoveRange(trans.Count - pos, pos);
                pos = 0;
            }
        }

        public void Undo()
        {
            while (pos < trans.Count)
            {
                var tt = trans[trans.Count - 1 - pos];
                if (tt.op == UITransactionOp.Checkpoint)
                {
                    pos++;
                    break;
                }
                tt.context.UndoTransaction(tt);
                pos++;
            }
        }
        public void Redo()
        {
            while (pos >= 0)
            {
                pos--;
                var tt = trans[trans.Count - 1 - pos];
                if(tt.op== UITransactionOp.Checkpoint)
                {
                    pos--;
                    break;
                }
                tt.context.PlayTransaction(tt);
            }
        }
        public void Clear()
        {
            trans.Clear();
        }
        public void Commit()
        {
            if (!Enabled)
                return;
            checkRestart();
            if (trans.Count > 0 && trans[trans.Count - 1].op == UITransactionOp.Checkpoint)
                return;
            UITransaction tr = new UITransaction();
            tr.op = UITransactionOp.Checkpoint;
            trans.Add(tr);
        }
        public bool Enabled
        {
            get;
            set;
        }
    }
    public interface IUITransactable
    {
        void PlayTransaction(UITransaction tr);
        void UndoTransaction(UITransaction tr);
    }
    public class UITransactableList<T> : IUITransactable where T : class
    {
        List<T> list = new List<T>(); // internal copy
        UITransactionManager tm;

        public UITransactableList(UITransactionManager tm)
        {
            this.tm = tm;
        }
        public T this[int index]
        {
            get { return list[index]; }
            set
            {
                tm.Transact(this,UITransactionOp.SetAt, index, new T[] { list[index], value });
                list[index] = value;
            }
        }
        public List<T> InternalList
        {
            get { return list; }
        }
        public void PlayTransaction(UITransaction tr)
        {
            switch(tr.op)
            {
                case UITransactionOp.SetAt :
                    list[tr.i1] = (T)tr.elems[1];
                    break;
                case UITransactionOp.Add :
                    list.Add((T)tr.elems[0]);
                    break;
                case UITransactionOp.AddRange :
                    foreach (var obj in tr.elems)
                        list.Add((T)obj);
                    break;
                case UITransactionOp.RemoveAt :
                    list.RemoveAt(tr.i1);
                    break;
                case UITransactionOp.RemoveRange :
                    list.RemoveRange(tr.i1, tr.i2);
                    break;
                case UITransactionOp.Clear :
                    list.Clear();
                    break;
                case UITransactionOp.InsertAt :
                    list.Insert(tr.i1, (T)tr.elems[0]);
                    break;
                default :
                    throw new NotImplementedException("playtrans Unsupported OP: " + tr.op);
            }
        }

        public void UndoTransaction(UITransaction tr)
        {
            int ix;
            switch (tr.op)
            {
                case UITransactionOp.SetAt :
                    list[tr.i1] = (T)tr.elems[0];
                    break;
                case UITransactionOp.Add :
                    list.RemoveAt(list.Count - 1);
                    break;
                case UITransactionOp.AddRange :
                    list.RemoveRange(list.Count - tr.i1, tr.i1);
                    break;
                case UITransactionOp.RemoveAt :
                    list.Insert(tr.i1, (T)tr.elems[0]);
                    break;
                case UITransactionOp.RemoveRange :
                    for (ix = 0; ix < tr.elems.Length;ix++ )
                    {
                        list.Insert(tr.i1 + ix, (T)tr.elems[ix]);
                    }
                    break;
                case UITransactionOp.Clear :
                    foreach (var obj in tr.elems)
                    {
                        list.Add((T)obj);
                    }
                    break;
                case UITransactionOp.InsertAt :
                    list.RemoveAt(tr.i1);
                    break;
                default:
                    throw new NotImplementedException("undotrans Unsupported OP: "+tr.op);
            }
        }

        public void Clear()
        {
            tm.Transact(this,UITransactionOp.Clear, list.ToArray());
            list.Clear();
        }
        public void Add(T obj)
        {
            tm.Transact(this,UITransactionOp.Add, new T[] { obj });
            list.Add(obj);
        }
        public void AddRange(IEnumerable<T> set)
        {
            List<T> temp = new List<T>();
            temp.AddRange(set);
            tm.Transact(this,UITransactionOp.AddRange, temp.Count, temp.ToArray());
            list.AddRange(set);
        }
        public void Insert(int idx,T obj)
        {
            tm.Transact(this,UITransactionOp.InsertAt, idx,new T[] { obj });
            list.Insert(idx, obj);
        }
        public void RemoveAt(int idx)
        {
            tm.Transact(this,UITransactionOp.RemoveAt, idx, new T[] { list[idx] });
            list.RemoveAt(idx);
        }
        public void Remove(T obj)
        {
            int index = list.IndexOf(obj);
            if (index < 0)
                return;
            RemoveAt(index);
        }
        public int Count
        {
            get { return list.Count; }
        }
        public void RemoveRange(int index,int count)
        {
            var r = list.GetRange(index, count);
            tm.Transact(this,UITransactionOp.RemoveRange, index, count, r.ToArray() );
            list.RemoveRange(index, count);
        }
    }

}
