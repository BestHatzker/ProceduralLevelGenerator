---
id: configFormat
title: Config format
sidebar_label: Config format
---

Config files are used when working with the GUI. They should cover most of the API of the C# library. They sometimes even support features that are not directly possible with the API.

# Reference

## Sections
Bellow you can find a list of all sections that can be used in config files. The order of sections is not important.

### `roomsRange` (required)
```yaml
roomsRange:
  from: # int, required
  to: # int, required
```

This section describe what rooms should be added to the map description. _to_ must be greater than _from_ - that mean that there must always be at least one room in the map description.

For example:
```yaml
roomsRange:
  from: 1      
  to: 10
```

---

### `passages` (required)
```yaml
passages:
  - # [int, int]   
```
This section describes passages between pairs of rooms. Map description represents an undirected graph so both `[x, y]` and `[y, x]` represent the same passage. Every passage can be added at most once.

Rooms together with passages must form a **connected graph**. That means that there must be a path between every pair of rooms.

For example:
```yaml
passages:
  - [1, 2]
  - [2, 5] 
```

**Note**: Passages are described as _yaml_ arrays suggesting that there could be more than two elements. But the parser expects every such array to contain **exactly** two elements and will throw an exception otherwise.

---

### `rooms` (optional)
```yaml
rooms:
  [array of room numbers]:
    roomShapes: # roomShapesModel, optional - see below
```

This section describes properties of individual rooms. It overrides default settings.

For example:
```yaml
rooms:
  [8]:
    roomShapes:
      -
        setName: bossRooms
        roomDescriptionName: bossRoom1
        scale: [2,2]
  [1,2,3]:
    roomShapes:
      -
        setName: basicRoom
        roomDescriptionName: basicRooms
```

**Note:** This section is **required** if defaults are not set.

**Typical usage:**
When all but one room should have common settings. Settings of that one special room can then be overriden in this section. See the [different shapes for some rooms](differentShapesMapDescription.md) documentation.

---

### `defaultRoomShapes` (optional)
```yaml
defaultRoomShapes: # [roomShapesModel - see below]
```

This section describes default room shapes to be used by all rooms. Values from this section are used only when they are not overriden in the _rooms_ section.

For example:
```yaml
defaultRoomShapes:
  -
    setName: rectangles
    roomDescriptionName: square
    rotate: false
  -
    setName: otherShapes
```

**Note:** This section is **required** to be non empty if not all rooms have their room shapes described in the _rooms_ section.

**Typical usage:** When there are many rooms that should use the same room shapes. Special rooms can then override defaults in the _rooms_ section.

### `corridors` (optional)
```yaml
corridors:
  enable: # bool, optional, default true
  offsets: # [positive int], required if enabled
  corridorShapes: # [roomShapesModel - see below], required if enabled
```

This section describes whether we want to add corridors between rooms. And if so, it lets us set them up.

- **enable**: Whether we want to enable corridors.
- **offsets**: What should be the distance between neighbouring non-corridor rooms. It would be often only a single number - the length of the corridor shape. But it is also possible to have multiple corridor shapes and thus multiple offsets. (See the Corridors tutorial for details about why is this field needed.)
- **corridorShapes**: Room shapes that we want to use for our corridors.

For example:
```yaml
corridors:
  enable: true
  offsets: [1]
  corridorShapes:
    -
      setName: tutorial_corridors
```

## Misc

### roomShapesModel
```yaml
setName: # string, optional
roomDescriptionName: # string, optional
rotate: # bool, optional
probability: # double, optional
normalizeProbabilities: # bool, optional
scale: # [positive int, positive int], optional
```

```yaml
setName: basicSet
roomDescriptionName: square
rotate: false
probability: 1.2
normalizeProbabilities: true
scale: [2,2]
```

- **setName**: Name of the set when having a room shapes set in a different file. Defaults to _custom_.
- **roomDescriptionName**: Name of the room description to be used. Defaults to all room descriptions in the set.
- **rotate**: Whether room shapes should be rotated. Defaults to _true_.
- **probability**: Probability of choosing the room shape when randomly perturbing shapes. Defaults to _1_.
- **normalizeProbabilities**: Whether probabilities should be normalized when rotating shapes. That means that if a room shape has _4_ different rotations the probability of each such rotation will be equal to _probability / 4_. Defaults to _true_.
- **scale**: Whether room shapes should be scaled. Defaults to _[1,1]_.
