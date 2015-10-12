TODO:

 - ITrackable 의 GetChildTrackable 를 외부로 분리하자. 여기에 있을 필요가 없다.
   여기에 있기 때문에 TrackableJsonExtentions.ApplyTo 의 대상이 ITrackable 이어야만 한다.
   사실 ApplyTo 대상은 object 면 충분하다.
   (예를 들면 TrackablePocoTracker\<Person\>
    는 TrackablePerson 뿐 아니라 Person 도 처리 가능 하기 때문에)

 - Trackable*TrackerJsonConverter 가 Generic 이 아니라 일반 타입으로 하자.
   이래야 T 별로 Converter 를 등록하는 수고를 해결할 수 있음

 - CodeGeneration 에 TrackableData 참조가 없으면 오류남
    ```
    System.Reflection.ReflectionTypeLoadException occurred
    'protobuf-net, Version=2.0.0.668, Culture=neutral, PublicKeyToken=257b51d87d2e4d67' 어셈블리에서 'TrackableData.ITrackablePoco' 형식을 로드할 수 없습니다.
    ```
    CodeGeneration 에 참조를 넣기만 하면 문제가 해결된다! 아 이거 찜찜해.
