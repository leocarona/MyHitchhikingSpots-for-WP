package md546d2b33f9a57d8a094bef67b2d4ad811;


public class SpotEditActivity
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onCreateDialog:(I)Landroid/app/Dialog;:GetOnCreateDialog_IHandler\n" +
			"";
		mono.android.Runtime.register ("MyHitchhikingSpots.Views.SpotEditActivity, MyHitchhikingSpots.Android, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", SpotEditActivity.class, __md_methods);
	}


	public SpotEditActivity () throws java.lang.Throwable
	{
		super ();
		if (getClass () == SpotEditActivity.class)
			mono.android.TypeManager.Activate ("MyHitchhikingSpots.Views.SpotEditActivity, MyHitchhikingSpots.Android, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public android.app.Dialog onCreateDialog (int p0)
	{
		return n_onCreateDialog (p0);
	}

	private native android.app.Dialog n_onCreateDialog (int p0);

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
