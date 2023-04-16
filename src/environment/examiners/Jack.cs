using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace JackNTFS.src.environment.examiners
{
    /* 该抽象类用于作为 广泛：“检验”
     * 被检验对象与检验对象为 “广泛”
     * 
     * (简单来说就是，有一种对象，他们具有某种可以用于互相检验同类对象的特征。
     *  我们把这种特征在这个抽象类中概括了。)
     * 
     * This abstract class is used to "EXAMINE" on GENERALISATIONS
     * Examinee and Examiner are GENERALISATIONS generalised.
     * 
     * :) *From William* */
    internal abstract class Jack<T>
    {
        private readonly T mJackType;
        private bool mIsJackable;

        public static T BeJackable(Jack<T> Jack)
        {
            Jack.mIsJackable = true;
            return Jack.mJackType;
        }

        public static bool IsItJack(Jack<T> Jack)
        {
            return (Jack is null);
        }

        protected Jack(T JackType, bool isJackable)
        {
            mJackType = JackType;
            mIsJackable = isJackable;
        }

        public bool IsJackable()
        {
            return mIsJackable;
        }
    }
}
