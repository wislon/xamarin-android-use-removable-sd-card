### Xamarin.Android: Use your removable SD card (KitKat+ too!)
Demonstrates how to how to access the 'external', __REMOVABLE__ SD card plugged into your Android device.

The code example is done in C#, but exactly the same principles apply in Java, so it should be extremely easy for an Android Java dev to understand exactly what's going on.

### Background
There's been a lot of questions on StackOverflow (and other places) about how to access the 'External Storage' on an Android device. Most people seem to want to know how to read from (and write to) __the SD card__ the've plugged into the SD card slot in their device. 

This sample project shows you how to do just that.

For more details, there's two blog posts about this that I've put together;

* [http://blog.wislon.io/posts/2014/09/28/xamarin-and-android-how-to-use-your-external-removable-sd-card](http://blog.wislon.io/posts/2014/09/28/xamarin-and-android-how-to-use-your-external-removable-sd-card) - how to figure out where it is, and access it pre-KitKat
* [http://blog.wislon.io/posts/2014/11/20/xamarin-and-android-kitkat-and-your-external-sd-card](http://blog.wislon.io/posts/2014/11/20/xamarin-and-android-kitkat-and-your-external-sd-card) - how to get access to it in KitKat and above, with some notes about backward compatibility.

_Everything in here is released as OSS under the MIT license, so feel free to use it any way you like._
