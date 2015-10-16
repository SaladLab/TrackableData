## TODO Mongo:

 - POCO
   - POCO 자체의 Serialization 가 잘 되는지
   - Id 필드 핸들링 되나 볼 것
   - Tracker 등 빠질 Attribute 처리
   - Default 값 저장 안하게
   - Update 해보자
 - PocoTracker 로 부터 Update Bson 만들기 
 - Dictionary
 - List

            // ObjectId 대신 long 도 사용할 수 있도록
## TODO SOON:

 - 유니티에서 잘 동작하는지 확인
   - json, protobuf 모두 확인 (Editor 는 잘 되네? Android 랑 iOS 보자.)

 - 기본 README 작성
   - 앞으로 작성할 매뉴얼의 큰 그림도 만들자

 - Mongo

 - Project 별로 Namespace 넣어주자.

## TODO LATER:

 - ITrackable 의 GetChildTrackable 를 외부로 분리하자. 여기에 있을 필요가 없다.
   여기에 있기 때문에 TrackableJsonExtentions.ApplyTo 의 대상이 ITrackable 이어야만 한다.
   사실 ApplyTo 대상은 object 면 충분하다.
   (예를 들면 TrackablePocoTracker\<Person\>
    는 TrackablePerson 뿐 아니라 Person 도 처리 가능 하기 때문에)

 - Trackable*TrackerJsonConverter 가 Generic 이 아니라 일반 타입으로 하자.
   이래야 T 별로 Converter 를 등록하는 수고를 해결할 수 있음

 - TrackableData-MsSql 이 Nullable 을 잘 지원하는지 보자.
