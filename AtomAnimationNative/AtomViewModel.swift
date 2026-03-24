import SwiftUI
import RealityKit
import Observation

@Observable
@MainActor
class AtomViewModel {
    var frames: [AtomFrame] = []
    var atomTypes: [String] = []
    var currentFrame: Int = 0
    var isPlaying: Bool = false
    var isLoading: Bool = false

    private(set) var atomEntities: [ModelEntity] = []
    private var animationTask: Task<Void, Never>?

    // Shared meshes — created once, reused for all atoms of each type
    private var hMesh: MeshResource?
    private var cMesh: MeshResource?
    private var hMaterial: SimpleMaterial?
    private var cMaterial: SimpleMaterial?

    func load() async {
        isLoading = true
        let result = await Task.detached(priority: .userInitiated) {
            XYZFrameLoader.loadAllFrames()
        }.value
        atomTypes = result.types
        frames = result.frames
        isLoading = false
    }

    func buildScene() -> AnchorEntity {
        // Place molecule 1.5 m high, 0.6 m in front of the user's origin
        let anchor = AnchorEntity(world: SIMD3<Float>(0, 1.5, -0.6))

        guard !frames.isEmpty else { return anchor }

        // Create shared meshes and materials once
        let cRadius: Float = 0.006
        let hRadius: Float = 0.004
        cMesh = MeshResource.generateSphere(radius: cRadius)
        hMesh = MeshResource.generateSphere(radius: hRadius)

        var cMat = SimpleMaterial(color: .init(red: 0.4, green: 0.4, blue: 0.45, alpha: 1), isMetallic: true)
        cMat.roughness = .float(0.3)
        var hMat = SimpleMaterial(color: .init(red: 0.95, green: 0.95, blue: 0.95, alpha: 1), isMetallic: false)
        hMat.roughness = .float(0.5)
        cMaterial = cMat
        hMaterial = hMat

        let firstFrame = frames[0]
        atomEntities = []
        atomEntities.reserveCapacity(atomTypes.count)

        for (i, type) in atomTypes.enumerated() {
            let isCarbon = type == "C"
            let mesh = isCarbon ? cMesh! : hMesh!
            let material = isCarbon ? cMat : hMat

            let entity = ModelEntity(mesh: mesh, materials: [material])
            if i < firstFrame.positions.count {
                entity.position = firstFrame.positions[i]
            }
            anchor.addChild(entity)
            atomEntities.append(entity)
        }

        return anchor
    }

    func updatePositions() {
        guard !frames.isEmpty, !atomEntities.isEmpty else { return }
        let frame = frames[currentFrame]
        for (i, entity) in atomEntities.enumerated() {
            if i < frame.positions.count {
                entity.position = frame.positions[i]
            }
        }
    }

    func play() {
        guard !isPlaying, !frames.isEmpty else { return }
        isPlaying = true
        animationTask = Task { @MainActor [weak self] in
            while let self, !Task.isCancelled, self.isPlaying {
                try? await Task.sleep(for: .milliseconds(50)) // 20 FPS
                self.currentFrame = (self.currentFrame + 1) % self.frames.count
            }
        }
    }

    func pause() {
        isPlaying = false
        animationTask?.cancel()
        animationTask = nil
    }
}
