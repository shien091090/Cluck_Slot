//粒子特效額外行為介面
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExtensibleParticle
{
    //(協程)特效參數初始化
    void Initialize(object param);
}
