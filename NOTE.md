## TODO SOON

### TrackableDictionary Modify Helper 추가

  Modify 가 딱 실수하기 좋게 생겼다. 
  - Functor 정도를 넣어서 자동 clone 을 해주는건 어떨까?
  - Clone 이 일반적이지 않다면 before, after 가 같으면 오류 내주는 것 정도를 넣어줘도 좋을 듯
  
  `dict.Update(key, (key, value) => return make_new_value)) => TrackModify(key, value, newValue)`
  `AddOrUpdate (TKey, TValue, Func<TKey, TValue, TValue>)` 를 넣어줘도 되겠다.

넣으면서 Upsert 를 넣어줘도 될 듯 

### Upsert 처리 정리

  Mongo 는 지금 대충 Upsert 하게 해놨는데 CRUD 시맨틱을 지키도록 잘 정리하자.

  - Create : 이미 존재하면 오류.
  - Delete : 삭제가 되던 말던 OK. 결과만 리턴
  - Load   : 없으면 NULL. (선택적으로 empty container 가 나올 수도 있다.)
  - Save   : 이미 존재할 경우만 UPDATE. 없으면 무시.

### 유니티에서 잘 동작하는지 확인

  json, protobuf 모두 확인 (Editor 는 잘 되네? Android 랑 iOS 보자.)

### 기본 README 작성

  앞으로 작성할 매뉴얼의 큰 그림도 만들자

### 고정 배열 처리?

  고정 배열 처리 (UserTeam.Members)

### 필드 무시 (ignore)

  필드 무시는 어떻게 할까?
  - Trackable 은 ignore 가 필요한가?
    - ProtoMember 혹은 JsonIgnore 과 같이 Plugin 마다 다르게 처리한다.
    - 만약 필요한게 있다면 (예: MsSql, Mongo) 알아서 정의해 사용하자.
  - Tracker 는 ignore 가 필요한가?
    - Tracking 이 필요하지 않는 것에 대해 필요하다.
    - 그렇다면 POCO 만 필요하다?
      - POCO 는 필요업다. 애초에 Interface 로 딱 필요한 것만 정의하도록 되어 있다.
        - Calculation method 는 partial class TrackablePoco 로 추가 가능하다.
      - TrackableDictionary 등에 일반 타입을 사용하는 것에 대해서는 해당 플러그인
        기능을 사용 (위 ProtoMember 등)

### 타입 지원

  주요 타입 IO 가 잘되나 보자.
  - TrackableData-MsSql 이 Nullable 을 잘 지원하는지 보자.

## TODO LATER

 - ITrackable 의 GetChildTrackable 를 외부로 분리하자. 여기에 있을 필요가 없다.
   여기에 있기 때문에 TrackableJsonExtentions.ApplyTo 의 대상이 ITrackable 이어야만 한다.
   사실 ApplyTo 대상은 object 면 충분하다.
   (예를 들면 TrackablePocoTracker\<Person\>
    는 TrackablePerson 뿐 아니라 Person 도 처리 가능 하기 때문에)

 - Trackable*TrackerJsonConverter 가 Generic 이 아니라 일반 타입으로 하자.
   이래야 T 별로 Converter 를 등록하는 수고를 해결할 수 있음

## Issues

### Nested Trackable

  중첩된 Trackable 에 대한 지원이 제한적이다.
  예를 들면 TrackableDictionary 의 TValue 로 TrackableDictionary 가 들어가는 등을 제대로 지원하지 않는다.
  이게 중요한 개발 피쳐가 아니었으며 가벼운 라이브러리에 반하는 기능이었기 때문에 제외되었다.
  나중에 개발할지도 모르니 이 이슈에 대해 정리를 해놓자.
