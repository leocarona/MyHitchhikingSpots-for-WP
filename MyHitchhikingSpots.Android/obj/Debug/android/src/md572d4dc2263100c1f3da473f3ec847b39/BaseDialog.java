package md572d4dc2263100c1f3da473f3ec847b39;


public class BaseDialog
	extends android.app.Dialog
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("MyHitchhikingSpots.Dialog.BaseDialog, MyHitchhikingSpots.Android, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", BaseDialog.class, __md_methods);
	}


	public BaseDialog (android.content.Context p0) throws java.lang.Throwable
	{
		super (p0);
		if (getClass () == BaseDialog.class)
			mono.android.TypeManager.Activate ("MyHitchhikingSpots.Dialog.BaseDialog, MyHitchhikingSpots.Android, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Android.Content.Context, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065", this, new java.lang.Object[] { p0 });
	}


	public BaseDialog (android.content.Context p0, boolean p1, android.content.DialogInterface.OnCancelListener p2) throws java.lang.Throwable
	{
		super (p0, p1, p2);
		if (getClass () == BaseDialog.class)
			mono.android.TypeManager.Activate ("MyHitchhikingSpots.Dialog.BaseDialog, MyHitchhikingSpots.Android, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Android.Content.Context, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:System.Boolean, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e:Android.Content.IDialogInterfaceOnCancelListener, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065", this, new java.lang.Object[] { p0, p1, p2 });
	}


	public BaseDialog (android.content.Context p0, int p1) throws java.lang.Throwable
	{
		super (p0, p1);
		if (getClass () == BaseDialog.class)
			mono.android.TypeManager.Activate ("MyHitchhikingSpots.Dialog.BaseDialog, MyHitchhikingSpots.Android, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Android.Content.Context, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:System.Int32, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", this, new java.lang.Object[] { p0, p1 });
	}

	java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
