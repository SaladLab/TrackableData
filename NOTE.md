## TODO SOON

## TODO LATER

 - ITrackable 의 GetChildTrackable 를 외부로 분리하자. 여기에 있을 필요가 없다.
   여기에 있기 때문에 TrackableJsonExtentions.ApplyTo 의 대상이 ITrackable 이어야만 한다.
   사실 ApplyTo 대상은 object 면 충분하다.
   (예를 들면 TrackablePocoTracker\<Person\>
    는 TrackablePerson 뿐 아니라 Person 도 처리 가능 하기 때문에)

## Issues

### Nested Trackable

  중첩된 Trackable 에 대한 지원이 제한적이다.
  예를 들면 TrackableDictionary 의 TValue 로 TrackableDictionary 가 들어가는 등을 제대로 지원하지 않는다.
  이게 중요한 개발 피쳐가 아니었으며 가벼운 라이브러리에 반하는 기능이었기 때문에 제외되었다.
  나중에 개발할지도 모르니 이 이슈에 대해 정리를 해놓자.
