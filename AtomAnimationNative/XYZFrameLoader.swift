import Foundation
import simd

struct AtomFrame {
    let positions: [SIMD3<Float>]
}

enum XYZFrameLoader {
    static func loadAllFrames() -> (types: [String], frames: [AtomFrame]) {
        var types: [String] = []
        var rawFrames: [[SIMD3<Float>]] = []
        var minPos = SIMD3<Float>(repeating:  Float.greatestFiniteMagnitude)
        var maxPos = SIMD3<Float>(repeating: -Float.greatestFiniteMagnitude)

        for i in 0..<201 {
            let name = String(format: "frame_%03d", i)
            guard
                let url = Bundle.main.url(forResource: name, withExtension: "txt", subdirectory: "txt_frames"),
                let content = try? String(contentsOf: url, encoding: .utf8)
            else {
                print("Missing frame: \(name)")
                continue
            }

            let lines = content.components(separatedBy: .newlines)
            var positions: [SIMD3<Float>] = []

            for j in 2..<lines.count {
                let line = lines[j].trimmingCharacters(in: .whitespaces)
                if line.isEmpty { continue }
                let parts = line.split(separator: " ", omittingEmptySubsequences: true)
                guard parts.count >= 4,
                      let x = Float(parts[1]),
                      let y = Float(parts[2]),
                      let z = Float(parts[3]) else { continue }

                let pos = SIMD3<Float>(x, y, z)
                positions.append(pos)

                if i == 0 {
                    types.append(String(parts[0]))
                }

                minPos = simd_min(minPos, pos)
                maxPos = simd_max(maxPos, pos)
            }

            rawFrames.append(positions)
        }

        guard !rawFrames.isEmpty else { return ([], []) }

        // Scale so the molecule fits in ~targetSize meters
        let center = (minPos + maxPos) * 0.5
        let extent = maxPos - minPos
        let maxExtent = max(extent.x, max(extent.y, extent.z))
        let targetSize: Float = 0.35
        let scale = maxExtent > 0 ? targetSize / maxExtent : 0.01

        let frames = rawFrames.map { positions in
            AtomFrame(positions: positions.map { ($0 - center) * scale })
        }

        return (types, frames)
    }
}
